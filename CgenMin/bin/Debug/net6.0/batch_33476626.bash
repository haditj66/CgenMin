#!/bin/bash
gnome-terminal -- sh -c "bash -c \"source ~/.bashrc;
 
cd /home/hadi/QR_Sync/world2/rosqt;
 
oursource;
 
cd -;
 
gnome-terminal -- bash -c
 
"ros2 run world2_rqt defaultTestRos  MySetting1; exec bash" ; exec bash\""