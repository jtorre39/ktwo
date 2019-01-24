while true; do
    pwd=$(pwd)
    instructions="This script will switch both branches to master. "
    echo "Please run this while in the root directory of the repository."
    echo $instructions
    read -p "Continue? [y\n]:" yn
    case $yn in
        [Yy]* ) 
            echo "Changing to both branches to master..."
            git checkout master
            cd unity-ktwo/Assets/ExternalAssets/ktwo-Assets
            git checkout master
            cd $pwd
            echo "Done!"
            break
            ;;
        [Nn]* ) 
            echo "Okie, cancelling..." && exit
            ;;
        * )  
            echo ""
            ;;
    esac
done