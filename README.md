# CollectiveBehaviour-project

The project was mainly based on Demšar J, Hemelrijk CK, Hildenbrandt H, Bajec IL (2015)Simulating predator attacks on schools: Evolving composite tactics and Demšar J, Lebar Bajec I (2014) Simulated predator attacks on flocks: a comparison of tactics. We wanted to reimplement and extend the predator and prey models presented, but without focusing on genetic algorithms. We have successfuly implemented the prey model, extended it to 3d and implemented a version that uses topological distance instead of metric. The predator behaviour was also successfuly implemented, but using rules from the 2014 paper.

# How to run

The project should be opened with the latest version of Unity. If they do not get installed automaticaly, the Entities and Hybrid Renderer packages may have to be installed through the package manager.

The settings for prey behaviour are in the fish_spawner GameObject, where all the parameters can be set. The number of prey agents that will be spawned is defined as Count^2. Other parameters are all set to be in line with the 2015 paper.

The settings for the predator are in SharkV1 GameObject. Its Mode defines its target selection tactic, 0 being attack closest, 1 being attack center and 2 being attack isolated.

To set the speed of simulation, set the timestep variable of the Globals class, in file FishSpawnerAuthoring.cs. A timestep of 0.1, which results in a 10x speedup, seems to work best for visualizations.

To run the simulation, press the play button on the top. The simulation will run for 600 timesteps with the selected parameters, then stop.
