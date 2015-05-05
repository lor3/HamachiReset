# Hamachi Service Reset
Windows service to periodically restart LogMeIn Hamachi service - attempts to restart the service and if unsuccessful restarts the machine.

Resolves issue where Hamachi (when used through http proxy) does not release TCP socket resources, eventually preventing the machine from opening any new connections.

See http://community.logmein.com/t5/Hamachi/Serious-issue-Hamachi-through-the-HTTP-proxy/td-p/81164 for more information.

Tested on/developed for Windows XP SP3 but should work fine on newer revisions.