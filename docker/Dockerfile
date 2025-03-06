# Use an official Ubuntu as a parent image
FROM ubuntu:22.04

# Set non-interactive mode for APT to prevent tzdata prompts
ENV DEBIAN_FRONTEND=noninteractive

# Install dependencies
RUN apt-get update && \
    apt-get install -y \
    tzdata gcc-10 g++-10 make cmake git \
    libmysqlclient-dev libssl-dev libboost-all-dev libace-dev \
    ninja-build wget libreadline-dev mysql-server liblua5.2-dev \
    mysql-client dos2unix net-tools iproute2 lsof && \
    rm -rf /var/lib/apt/lists/*

# Set the timezone explicitly
RUN ln -fs /usr/share/zoneinfo/Etc/UTC /etc/localtime && \
    echo "Etc/UTC" > /etc/timezone && \
    dpkg-reconfigure -f noninteractive tzdata

# Set GCC 10 as default
RUN update-alternatives --install /usr/bin/gcc gcc /usr/bin/gcc-10 100 && \
    update-alternatives --install /usr/bin/g++ g++ /usr/bin/g++-10 100

# Install newer CMake
RUN wget https://github.com/Kitware/CMake/releases/download/v3.22.0/cmake-3.22.0-linux-x86_64.tar.gz && \
    tar -xvf cmake-3.22.0-linux-x86_64.tar.gz --strip-components=1 -C /usr/local && \
    rm cmake-3.22.0-linux-x86_64.tar.gz

# Clone the MangosZero repositories
RUN git clone --recursive https://github.com/mangoszero/server.git /sources
RUN git clone --recursive https://github.com/mangoszero/database.git /database

# Build and install MangosZero
WORKDIR /sources
RUN mkdir build && cd build && \
    /usr/local/bin/cmake -G Ninja .. -DCMAKE_INSTALL_PREFIX=/mangos -DCMAKE_VERBOSE_MAKEFILE=OFF && \
    ninja -j$(nproc) && \
    ninja install

# Ensure configuration directory exists
RUN mkdir -p /mangos/etc /var/lib/mysql

# Copy default configuration files if they exist
RUN test -f /usr/local/mangoszero/etc/mangosd.conf.dist && cp /usr/local/mangoszero/etc/mangosd.conf.dist /usr/local/mangoszero/etc/mangosd.conf || true
RUN test -f /usr/local/mangoszero/etc/realmd.conf.dist && cp /usr/local/mangoszero/etc/realmd.conf.dist /usr/local/mangoszero/etc/realmd.conf || true

# Create install directory and copy scripts
RUN mkdir -p /install
COPY startup.sh /usr/local/bin/startup.sh
COPY InstallDatabases.sh /install/InstallDatabases.sh
COPY InstallWorld.sh /install/InstallWorld.sh
COPY UpdateDatabases.sh /install/UpdateDatabases.sh
COPY UpdateWanIP.sh /install/UpdateWanIP.sh
COPY wip.sh /install/wip.sh
RUN dos2unix /install/*.sh && chmod +x /install/*.sh && dos2unix /usr/local/bin/startup.sh && chmod +x /usr/local/bin/startup.sh

# Ensure liblua52.so is available
RUN ln -s /usr/lib/x86_64-linux-gnu/liblua5.2.so /usr/lib/x86_64-linux-gnu/liblua52.so

# Copy configuration files to the correct location
COPY mangosd.conf /mangos/etc/mangosd.conf
COPY realmd.conf /mangos/etc/realmd.conf
COPY console.conf /mangos/etc/console.conf

# Expose necessary ports
EXPOSE 3724 8085 3306 3443

# Set CMD to run startup script
ENTRYPOINT ["/usr/local/bin/startup.sh"]
