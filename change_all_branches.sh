cwd=$(pwd)
echo "Please run this while in the root directory of the repository."
read -p "Change all repos to which branch? [branch name]: " branchName

while true; do
 read -p "Change all branches to $branchName ? [y\n]: " yn
    case $yn in
        [Yy]* ) git checkout "$branchName"
        cd unity-ktwo/assets/ExternalAssets/assets-ktwo
        git checkout "$branchName"
        cd "$cwd"
        break;;
        [Nn]* ) echo "Okie, cancelling..." && break;;
        * ) echo "Please enter: [y\n]";;
    esac
done
