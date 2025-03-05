#!/bin/bash

set -e

# Ensure MySQL user has a valid home directory (fixing Dockerfile)
if [ "$(getent passwd mysql)" ]; then
    usermod -d /var/lib/mysql mysql || true
fi

# Ensure MySQL data directory exists (fixing Dockerfile)
mkdir -p /var/lib/mysql
chown -R mysql:mysql /var/lib/mysql
chmod 700 /var/lib/mysql

# Clean MySQL data directory if it contains files
if [ "$(ls -A /var/lib/mysql)" ]; then
    echo "Cleaning MySQL data directory..."
    rm -rf /var/lib/mysql/*
fi

# Ensure necessary shared libraries are available (fixing startup.sh)
export LD_PRELOAD="/usr/lib/x86_64-linux-gnu/liblua5.2.so"

# Prevent repeated MySQL initialization (same as before)
if [ ! -f "/var/lib/mysql/ibdata1" ]; then
    echo "Initializing MySQL data directory..."
    mysqld --initialize-insecure --user=mysql --datadir=/var/lib/mysql || (echo "MySQL initialization failed. Checking logs..." && cat /var/log/mysql/error.log && exit 1)
else
    echo "MySQL data directory already initialized. Skipping..."
fi

# Stop any existing MySQL processes (same as before)
echo "Stopping any existing MySQL processes..."
service mysql stop || true
pkill -f mysqld || true

# Remove leftover socket files
rm -f /var/run/mysqld/mysqld.sock /var/run/mysqld.mysqlx.sock

# Verify MySQL is running and logs exist (same as before)
if [ ! -f /var/log/mysql/error.log ]; then
    echo "MySQL service did not start or log file does not exist. Please check the logs."
    exit 1
fi

# Start MySQL service
echo "Starting MySQL service..."
service mysql start || (echo "MySQL failed to start. Checking logs..." && cat /var/log/mysql/error.log && exit 1)

# Wait for MySQL to be ready
until mysqladmin ping -h "localhost" --silent; do
    echo "Waiting for MySQL to start..."
    sleep 2
done

# Manually set the root password
mysql -uroot -e "ALTER USER 'root'@'localhost' IDENTIFIED BY '${MYSQL_ROOT_PASSWORD}';"

# Ensure MySQL user and databases are properly set up
mysql -uroot -p${MYSQL_ROOT_PASSWORD} -e "CREATE DATABASE IF NOT EXISTS mangos0;"
mysql -uroot -p${MYSQL_ROOT_PASSWORD} -e "CREATE DATABASE IF NOT EXISTS character0;"
mysql -uroot -p${MYSQL_ROOT_PASSWORD} -e "CREATE DATABASE IF NOT EXISTS realmd;"
mysql -uroot -p${MYSQL_ROOT_PASSWORD} -e "CREATE USER IF NOT EXISTS 'mangos'@'%' IDENTIFIED BY 'mangos';"
mysql -uroot -p${MYSQL_ROOT_PASSWORD} -e "GRANT ALL PRIVILEGES ON *.* TO 'mangos'@'%' WITH GRANT OPTION;"
mysql -uroot -p${MYSQL_ROOT_PASSWORD} -e "FLUSH PRIVILEGES;"

# Create symbolic links for game data
ln -s /game-data/maps /mangos/maps
ln -s /game-data/vmaps /mangos/vmaps
ln -s /game-data/mmaps /mangos/mmaps
ln -s /game-data/dbc /mangos/dbc

# Run InstallDatabases.sh with expect to provide inputs and log output to a separate file
/install/InstallDatabases.sh

# Ensure necessary shared libraries are available
ldconfig

# Print message to indicate logs are visible
echo "Starting realmd and mangosd in foreground mode for TrueNAS logs..."

cd /mangos
# Run both services in foreground so logs are visible in TrueNAS UI
/mangos/bin/realmd -c /mangos/etc/realmd.conf &
exec /mangos/bin/mangosd -c /mangos/etc/mangosd.conf
