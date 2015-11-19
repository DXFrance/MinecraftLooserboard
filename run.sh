#!/bin/bash

# start minecraft server in background

cd /opt/minecraft/

java -Xmx1024M -Xms1024M -jar minecraft_server.jar nogui &

# start kestrel server for looserboard

cd /opt/looserboard
dnx kestrel