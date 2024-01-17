#!/bin/bash
LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/usr/lib/pororoca pororoca_executable

# The command
# LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/usr/lib/pororoca
# is necessary to add Pororoca .so libraries to LD_LIBRARY_PATH,
# in order to make them available for main Pororoca executable 
# during initialization.
# This makes this environment variable set in a command scope only.
# https://unix.stackexchange.com/a/285206
# https://stackoverflow.com/a/68299178
# https://stackoverflow.com/a/18549117
