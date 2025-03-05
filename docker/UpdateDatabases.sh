#!/bin/sh
svr="localhost"
user="root"
pass=$MYSQL_ROOT_PASSWORD
port="3306"
wdb="mangos0"
cdb="character0"
rdb="realmd"

UpdateWorld()
{
        printf "Updating data into the World database ${wdb}\n"
        for file in $(find /database/World/Updates/ -name '*.sql' -print)
        do
                printf "Applying update ${file}\n"
                mysql -u $user -p$pass -s ${wdb} < ${file}
        done
}

UpdateRealm()
{
        printf "Updating data into the Realm database ${rdb}\n"
        for file in $(find /database/Realm/Updates/ -name '*.sql' -print | grep -v 2015)
        do
                printf "Applying update ${file}\n"
                mysql -u $user -p$pass -s ${rdb} < ${file}
        done
}

UpdateCharacter()
{
        printf "Updating data into the Character database ${db}\n"
        for file in $(find /database/Character/Updates/ -name '*.sql' -print)
        do
                printf "Applying update ${file}\n"
                mysql -u $user -p$pass -s ${cdb} < ${file}
        done
}


#the update files check if they are valid themselves
#this means we don't need to know the order in which we install the updates
#we repeat 25 times to make sure all updates are done
#it will pick the right update each time we try.
#lazy i know, but it works.
#if after these updates the container won't start due to not detecting world database being update
#run /install/InstallWorld.sh to fix it
#make backups 'n all
for i in $(seq 1 25) ; do UpdateWorld ; done
for i in $(seq 1 25) ; do UpdateRealm ; done
for i in $(seq 1 25) ; do UpdateCharacter ; done
