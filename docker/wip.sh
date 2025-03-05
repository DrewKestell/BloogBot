svr="localhost"
user="root"
pass=$MYSQL_ROOT_PASSWORD
port="3306"
wdb="mangos0"
cdb="character0"
rdb="realmd"

MANGOS_BINARY_VER=$(/mangos/mangosd --version)
REALMD_BINARY_VER=$(/mangos/realmd --version)

echo $MANGOS_BINARY_VER
echo $REALMD_BINARY_VER

mysql -u root -p$pass -e "show databases"
mysql -u root -p$pass -se "SELECT version FROM db_version" realmd | grep -v version | tail -n1
mysql -u root -p$pass -se "SELECT structure FROM db_version" realmd | grep -v structure | tail -n1
mysql -u root -p$pass -se "SELECT content FROM db_version" realmd | grep -v content | tail -n1