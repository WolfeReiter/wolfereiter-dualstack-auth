#!/bin/sh
if [ ! -f "`which npm`" ]; then
    echo "You need to install npm. <https://www.npmjs.com/get-npm>"
    echo ""
    echo "With MacPorts:"
    echo "sudo port install npm6"
    exit 1
fi

cd src/WolfeReiter.Identity.DualStack/
rm -fr node_modules
npm ci
echo "Done."