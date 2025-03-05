#!/bin/sh
##############################################################################
# This utility assist you in setting up your mangos database.                #
# This is a port of InstallDatabases.bat written by Antz for Windows         #
#                                                                            #
##############################################################################


RELEASE="Rel21"
UPDATES="Rel20_to_BaseRel21_Updates"
DUMP="NO"

createcharDB="YES"
createworldDB="YES"
createrealmDB="YES"

loadcharDB="YES"
loadworldDB="YES"
loadrealmDB="YES"
dbType="POPULATED"

updatecharDB="YES"
updateworldDB="YES"
updaterealmDB="YES"

addRealmList="YES"

svr="localhost"
user="root"
pass=$MYSQL_ROOT_PASSWORD
port="3306"
wdb="mangos0"
cdb="character0"
rdb="realmd"

printHelp()
{
	printf "Usage: $0 [-s] [-u] [-d] [-h]\n"
	printf "\t-s: Run this script in silent mode, only prompt for the database information\n"
	printf "\t-u: Run only the updates of the database\n"
	printf "\t-d: Dump the database configuration into the home directory of the user\n"
	printf "\t-h: Display this help\n"
}

printBanner()
{
	clear
	printf " #     #     #    #   #  ###   ###   ####  \n"
	printf " ##   ##    # #   ##  # #     #   # #      \n"
	printf " # # # #   #   #  # # # # ### #   #  ###   \n"
	printf " #  #  #  ####### #  ## #   # #   #     #  \n"
	printf " #     #  #     # #   #  ###   ###  ####   \n"
	printf "\n"
	printf " Database Setup and World Loader v0.01 	\n"
	printf " ---------------------------------------------- \n"
	printf "  Website / Forum / Wiki : https://getmangos.eu \n"
	printf " ---------------------------------------------- \n"
	printf "\n"
	printf "\n"
}

printActivities()
{
	printf "\tCharacter Database : \tV - Toggle Create DB (${createcharDB})	\n"
	printf "\t\t\t\tC - Toggle Create Structure (${loadcharDB}) \n"
	printf "\t\t\t\tB - Apply Character DB updates (${updatecharDB})\n"
	printf "\n"
	printf "\t    World Database : \tE - Toggle Create DB (${createworldDB}) \n"
	printf "\t\t\t\tW - Toggle Create Structure (${loadworldDB}) \n"	
	if [ "${loadworldDB}" = "YES" ]; then
		printf "\t\t\t\tD - Toggle World Type (${dbType})\n"
	fi
	printf "\t\t\t\tU - Apply World DB updates (${updateworldDB})\n"
	printf "\n"
	printf "\t    Realm Database : \tT - Toggle Create DB (${createrealmDB})\n"
	printf "\t\t\t\tR - Toggle Create Structure (${loadrealmDB})\n"
	printf "\t\t\t\tY - Apply Realm DB updates (${updaterealmDB})\n"
	printf "\t\t\t\tL - Toggle Add RealmList Entry (${addRealmList})\n"	
	printf "\n"
	printf "\t\t\t\tN - Next Step\n"
	printf "\t\t\t\tX - Exit\n"
}

createCharDB()
{
	printf "Creating Character database ${cdb}\n"
	mysql -u $user -p$pass -q -s -e "Create database ${cdb}"
	if [ "${loadcharDB}" = "YES" ]; then
		loadCharDB
	fi
}

loadCharDB()
{
	printf "Loading data into character database ${cdb}\n"
	mysql -u $user -p$pass -q -s ${cdb} < /database/Character/Setup/characterLoadDB.sql
}

updateCharDB()
{
	printf "Updating data into the character database ${cdb}\n"
	for file in $(ls /database/Character/Updates/*.sql | tr ' ' '|' | tr '\n' ' ')
	do
		file=$(echo ${file} | tr '|' ' ')
		printf "Applying update ${file}\n"
		mysql -u $user -p$pass -q -s ${cdb} < ${file}
	done
}

createWorldDB()
{
	printf "Creating World database ${wdb}\n"
	mysql -u $user -p$pass -q -s -e "Create database ${wdb}"
	if [ "${loadworldDB}" = "YES" ]; then
		loadWorldDB
	fi
}

loadWorldDB()
{
	printf "Loading data into world database ${wdb}\n"
	mysql -u $user -p$pass -q -s ${wdb} < /database/World/Setup/mangosdLoadDB.sql
	
	if [ "${dbType}" = "POPULATED" ]; then
		populateWorldDB
	fi
}

populateWorldDB()
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

updateWorldDB()
{
	printf "Updating data into the World database ${wdb}\n"
	for file in $(ls /database/World/Updates/*.sql | tr ' ' '|' | tr '\n' ' ')
	do
		file=$(echo ${file} | tr '|' ' ')
		printf "Applying update ${file}\n"
		mysql -u $user -p$pass -q -s ${wdb} < ${file}
	done
}

createRealmDB()
{
	printf "Creating realm database ${rdb}\n"
	mysql -u $user -p$pass -q -s -e "Create database ${rdb}"
	if [ "${loadrealmDB}" = "YES" ]; then
		loadRealmDB
	fi
}

loadRealmDB()
{
	printf "Loading data into realm database ${rdb}\n"	
	mysql -u $user -p$pass -q -s ${rdb} < /database/Realm/Setup/realmdLoadDB.sql
}

updateRealmDB()
{
	printf "Updating data into the Realm database ${rdb}\n"
	for file in $(ls /database/Realm/Updates/*.sql | tr ' ' '|' | tr '\n' ' ')
	do
		file=$(echo ${file} | tr '|' ' ')
		printf "Applying update ${file}\n"
		mysql -u $user -p$pass -q -s ${wdb} < ${file}
	done
}

addRealmList()
{
	printf "Adding realm list entries\n"
	mysql -u $user -p$pass -q -s ${rdb} < /database/Tools/updateRealm.sql
}



if [ "${createcharDB}" = "YES" ]; then
	createCharDB
fi

if [ "${createworldDB}" = "YES" ]; then
	createWorldDB
fi

if [ "${createrealmDB}" = "YES" ]; then
	createRealmDB
fi

if [ "${updatecharDB}" = "YES" ]; then
	updateCharDB
fi

if [ "${updateworldDB}" = "YES" ]; then
	updateWorldDB
fi

if [ "${updaterealmDB}" = "YES" ]; then
	updateRealmDB
fi

if [ "${addRealmList}" = "YES" ]; then
	addRealmList
fi

if [ "${DUMP}" = "YES" ]; then
	printf "Dumping database information...\n"
	echo "${svr};${port};${user};${pass};${rdb}" > ~/db.conf
	echo "${svr};${port};${user};${pass};${wdb}" >> ~/db.conf
	echo "${svr};${port};${user};${pass};${cdb}" >> ~/db.conf
fi


printf "Database creation and load complete :-)\n"
printf "\n"
printf "Updating Docker Host IP and/or WAN IP into realm database\n"
printf "\n"
echo mysql -u root -pmangos -e \'update realmd.realmlist SET localAddress='"'$DOCKER_HOST_IP'" 'WHERE id='1'';'\' > /install/updateip.sh
echo mysql -u root -pmangos -e \'update realmd.realmlist SET address='"'$WAN_IP_ADDRESS'" 'WHERE id='1'';'\' >> /install/updateip.sh
chmod +x /install/updateip.sh
/install/updateip.sh
printf "Updated Docker Host IP into realm database\n"
printf "\n"
