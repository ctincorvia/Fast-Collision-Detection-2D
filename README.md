# Fast-Collision-Detection-2D
An interface  and implementation for heavily optimized collision detection in 2D simulations

# How to use:
Any object that implements the ICollidable interface can make use of the fast collision detection algorithm.
Once you've created a class that implements the ICollidable interface, initialize a Map objects and add all the objects you hope to collide in to it.
Then, every time you run DetectCollisions() all pairs of objects that collide with each other are returned.
The first call to DetectCollisions() is significantly more expensive than any other call (Threads must be spun up and objects must be put to sleep) so it is recommended it's called the first time during a non-critical section of the code.

# Implementation details
In order to improve performace, several optimizations are made.
First, the space in which collisions can occur is divided in to zones.  When detecting collisions, objects are only compared to objects in their zones and adjacent zones.  
Second, objects that are far from all other objects are put to sleep for an amount of time based on their distance from the nearest object.
Sleeping objects are not considered for collisions.
Third, objects that are close enough to each other that they could collide are checked for "coarse" collisions, where the maximum reach of an object from it's center is compared to the other object.  If this comparison yeilds that it is not possible for the objects to collide, the collision is ignored.  This allowes potentially expensive exact collision detection (using meshes, etc) to be performed only when absolutely necessary.

# Hints
If an object is so large it can collide with another object outside of it's zone and adjacent zones, collision detection will fail.  Reduce the number of zones so this is not possible. 
