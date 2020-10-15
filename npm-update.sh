#!/bin/sh
if [ ! -f "`which npm`" ]; then
    echo "You need to install npm. <https://www.npmjs.com/get-npm>"
    echo ""
    echo "With MacPorts:"
    echo "sudo port install npm6"
    exit 1
fi

echo "Updating npm packages..."
cd src/WolfeReiter.Identity.DualStack/
npm update
echo "Done."