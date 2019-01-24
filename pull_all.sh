while true; do
    cwd=$(pwd)
    echo "Please run this while in the root directory of the repository."
    read -p "Pull on current branches for all repositories? [y\n]: " yn
    case $yn in
        [Yy]* ) git pull && cd unity/ktwo/assets/ExternalAssets/ktwo-assets && git pull && cd $cwd && exit;;
        [Nn]* ) echo "Okie, cancelling..." && exit;;
        * ) echo "Please enter: [y\n]";;
    esac
done