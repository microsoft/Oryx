#!/bin/sh
# This script generates fresh pairs of ssh keys

# generate fresh rsa key
echo "y" | ssh-keygen -f /etc/ssh/ssh_host_rsa_key -N '' -t rsa

# generate fresh dsa key
echo "y" | ssh-keygen -f /etc/ssh/ssh_host_dsa_key -N '' -t dsa

# generate fresh ecdsa key
echo "y" | ssh-keygen -f /etc/ssh/ssh_host_ecdsa_key -N '' -t ecdsa

# generate fresh ecdsa key
echo "y" | ssh-keygen -f /etc/ssh/ssh_host_ed25519_key -N '' -t ed25519

# prepare run dir
mkdir -p /var/run/sshd