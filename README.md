

# CgenMin

This is a collection of various dev tools. Code generation, QRCore tools, command handlers, etc are some examples of these dev tools.
 ![output image](OutputImage.png)

 
# Install Instructions
NOTE: only clone repo for one OS. Then use syncthing to sync the files over to the other OS.

### windows 
- Step 1:  
   - in your C:/ directory create a directory named `QR_Sync`
- Step 2:  in a terminal run the following commands 
   `cd C:/QR_Sync`
  `git clone git@github.com:haditj66/CgenMin.git` 
-  Step 3: 
  - got to your environment directory and add the a variable named cgen to C:/QR_Sync/CgenMin/CgenMin/bin/Debug/net6.0 
HADI TODO:  You'll need to change the exe ouput to cgen

To verify installation, open a new terminal and enter cgen. You should get the help output with available commands.
 
 

### Linux
Make sure you have dotnet runtime installed for dotnet 6.0.0  . Depending on your linux version, you may have to run the following command first
 `sudo add-apt-repository ppa:dotnet/backports`
and then 
 `sudo apt-get update && \
   sudo apt-get install -y dotnet-sdk-6.0`
and then 
sudo apt-get install -y dotnet-runtime-6.0
- Step 1: 
  - for linux: in your home directory create a directory named `QR_Sync` 
- Step 2:  in a terminal run the following commands 
  - `cd ~/QR_Sync` 
  - `git clone git@github.com:haditj66/CgenMin.git`
- Step 3:  
  - copy the contents in PutInDotbashrc.txt located in CgenMin directory. And APPEND (not replace) it into the .bashrc file located in your home directory.
 
 ---
- Step 4: 
 You might need to change something in PutInDotbashrc.txt . Around line 9 where it says 
 source /opt/ros/foxy/setup.bash
 make sure this is the ROS2 command for sourcing your specific ROS2 installation.
 
 ---
- Step 5 (optional): Verify it worked by opening a new terminal and typing "cgen". you should get the help menu for it.
 
