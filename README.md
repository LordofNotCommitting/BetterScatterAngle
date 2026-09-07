The patch 1.02 made scatter "Visually correct" as per ingame mechanics.
but it also look kind of terrible as all bullets get railed into exact dead center of the tile. Unfortunate, but that is how game functions. But let's see if I can fix that.

**Need to restart the game after MCM config setup for this mod to take effect**

This mod enforces more accurate scatter that is less bound to ingame tile system(now it aims at very far arbitrary location) while keeping the visual synced to bullet.

**This means this mod changes ingame mechanics.** In vanilla Guns just magnetized bullet to close wall. Now it will not. 
All guns will have more accurate scatter as intended **but this means all guns are now more accurate as it is not gimped by vanilla scatter mechanics.**

This impact all guns. Enemies too. You have been warned.

This mod also offer Gaussian projectile distribution option... which just means now bullet is centered instead of being completely randomly scattered. See pic above. Distribution value configurable on MCM.

As both of those options make all guns REALLY ACCURATE. I have added option to nerf scatter to compensate. Also controllable on MCM.

For v1.1. Now this mod support new target hit system based on trace pathing instead of cellposition pathing which impacted high speed projectile (or if you used my mod, "Optimize Projectile Processing" which sped them up) by making diagonal projectile phase through some tiles. Now enemy is something like a circle within square 1x1 cell instead of entire cell (configurable on MCM) and will be hit if it is in the path of a projectile instead of phasing through.

Oh, and because now everyone has a hitbox. Diagonal projectiles are slightly harder to hit. If you want vanilla behavior then change hitbox size to 0.71(entire cell which is vanilla intended) or disable this feature.

Changelog:

v1.11:
Moved scatter penalty somewhere else so it works with scatter display mod.

v1.1:

Now this mod support new target hit system based on trace pathing instead of cellposition pathing which impacted high speed projectile by making diagonal projectile phase through some tiles.

v1.01:

For Scatter penalty. 1.00 was actually 100%. Changed scatter % to integer. And scatter penalty's 1 = 1% now.

The 2nd picture uses "Scatter Indicator" mod for testing.
https://steamcommunity.com/sharedfiles/filedetails/?id=3785407853
