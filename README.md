# Fibonacci-Sphere
This unity project creates an evenly distributed array of points on the surface of a sphere. I created it with the intention of using it to simulate weather, but haven't gotten to the weather simulation part yet.

Currently, the left sphere holds the script that creates the points, and they are visible by using the Gizmos. During runtime the script checks if it has enough points, and generates a new array if it doesn't. The points are distributed evenly over a sphere, and have information about their neighbors. Later I want to accelerate the process by using GPU compute-shaders, but for now my Ryzen 7 3.59 GHz processor seems to get up to about 5000 points before it really starts to choke. That is probably in part because the Gizmos render ALL of them at once.

I'll add some resources to this readme later with sources that I found useful while making this project.
