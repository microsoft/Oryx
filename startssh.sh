if [ "$WEBSITE_SSH_ENABLED" != "0" ]; then
    # 2>&1 - merges the output from stdout and stderr.
    # /dev/null - pseudo-devices special file which accepts and discards all inputs, produces no output.
    # In this case, ssh_setup.sh runs ssh-key which emits logs to the stdout. These contain some PII information and hence we want discard all the output using the following command.
    /opt/startup/ssh_setup.sh 2>&1 > /dev/null
    sed -i "s/SSH_PORT/$SSH_PORT/g" /etc/ssh/sshd_config
    service ssh start
fi