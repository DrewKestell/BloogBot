#!/bin/sh
#sometimes updating the world DB doesn't suffice.
#Then it needs to be reinstalled.
#The world DB doesn't contain userdata.
#This means it is always safe to drop and recreate this database

svr="localhost"
user="root"
pass=$MYSQL_ROOT_PASSWORD
port="3306"
wdb="mangos0"
cdb="character0"
rdb="realmd"

DropWorld()
{
	printf "Dropping World Database"
	mysql -u $user -p$pass -q -s -e "Drop database ${wdb}"
}
CreateWorld()
{
	printf "Creating World database ${wdb}\n"
	mysql -u $user -p$pass -q -s -e "Create database ${wdb}"
}

LoadWorld()
{

	printf "Loading data into world database ${wdb}\n"
	mysql -u $user -p$pass -q -s ${wdb} < /database/World/Setup/mangosdLoadDB.sql
}

ImportWorld()
{
	printf "Importing World database ${wdb}\n"
	for file in $(ls /database/World/Setup/FullDB/*.sql | tr ' ' '|' | tr '\n' ' ') 
	do
		file=$(echo ${file} | tr '|' ' ')
		printf "Importing file ${file}\n"
		mysql -u $user -p$pass -q -s ${wdb} < ${file}
		printf "File ${file} imported\n"
	done
}

UpdateWorld()
{
	printf "Updating data into the World database ${wdb}\n"
	for file in $(find /database/World/Updates/ -name '*.sql' -print)
	do
		printf "Applying update ${file}\n"
		mysql -u $user -p$pass -s ${wdb} < ${file}
	done
}

DropWorld
CreateWorld
LoadWorld
ImportWorld
for i in $(seq 1 25) ; do UpdateWorld ; done