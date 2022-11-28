Design Decisions:

	BEAM ANIMATION:	The beam fires from the end of the turret and the width of beam decreases as 
	the animation progresses. Circles are drawn on both sides of the beam and move away from the beam
	as the animation progresses.

	TANK EXPLOSION: For the tank dying animation we draw 8 circles that move away from the center of the dead 
	tank's location as the animation progresses. 

	PROJECTILES: We use the same color projectile as the owner tank and turret. 

	HEALTH BAR:  Changes the color and size of the health bar, depending on the player's hp.

	PLAYER MOVEMENT: We keep track of players key inputs by storing them in a stack. This way player movements
	work when holding multiple keys.

	PLAYER NAMES: We center player's names and scores below the tank so that the name is centered regardless of the
	length of the name.

PS9 README:

	We implemplemented our server to have similar behaviour to the provided example. In addition, we made the following modifications
	to the server:
	
		WRAPAROUND: When a tank reaches the end of the world, it will be teleported to the opposite side. However if there
		is a wall on the other side that would cause a collision, then the tank's location will not change.

		EXTRA SETTINGS: Changing the settings xml file modifies the physics of the game. In addition, we added extra settings
		that can be modified including, tank's MaxHP, projectile speed, tank's engine strength, tank's size, wall's size, max powerups
		and the max powerup delay. 

		COLLISIONS: When we check for collisions, we made the border of the objects 1 unit bigger than the actual size in order for
		a better visualization of the collision. (able to see space between objects)

		ERRORS: When an error occurs when communicating with clients, we print an error message to the console that includes the client's 
		ID. We also print whenever a client disconnects.


	




