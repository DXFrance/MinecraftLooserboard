############################################################
# Dockerfile to run ASP.NET 5 + Minecraft Server
# Based on Microsoft ASP.NET Beta RC1 Image for coreCLR
############################################################

# use the Aspnet Beta RC1 base image for coreCLR
FROM microsoft/aspnet:1.0.0-rc1-final-coreclr

# Set the file maintainer
MAINTAINER Julien CORIOLAND, Benjamin TALMARD

# Create environment variables
ENV MINECRAFT_SERVER_DIR /opt/minecraft
ENV LO0SERBOARD_SRC_DIR /opt/looserboard

# Create the directories
RUN mkdir -p $MINECRAFT_SERVER_DIR
RUN mkdir -p $LO0SERBOARD_SRC_DIR

###################
#                 #
# LOOSERBOARD     #
#                 #
###################

WORKDIR $LO0SERBOARD_SRC_DIR

# copy the content of the web app
COPY ./aspnet-looserboard $LO0SERBOARD_SRC_DIR/

# restore the nuget packages
RUN dnu restore

# expose the port 5000, the default one used by Kestrel server
EXPOSE 5000

###################
#                 #
# MINECRAFT       #
#                 #
###################

# Install the latest version of the JDK, wget
RUN apt-get -qq update && apt-get -qqy install default-jdk wget

# Set the minecraft server directory as working directory
WORKDIR $MINECRAFT_SERVER_DIR

# Download the minecraft server
RUN wget -O minecraft_server.jar https://s3.amazonaws.com/Minecraft.Download/versions/1.8.7/minecraft_server.1.8.7.jar

# Copy Eula.txt & server.properties into directory
COPY ./minecraft/eula.txt $MINECRAFT_SERVER_DIR/
COPY ./minecraft/server.properties $MINECRAFT_SERVER_DIR/

# Expose the ports #25565 outside the container
EXPOSE 25565

ADD run.sh /run.sh
RUN chmod +x /run.sh

WORKDIR /

# start minecraft server & aspnet looserboard
ENTRYPOINT ["/run.sh"]