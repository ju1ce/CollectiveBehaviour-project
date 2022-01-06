using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(FishSystem))]
public partial class MovementSystem : SystemBase
{

    string path;
    string run;
    bool enabled;
    bool write;

    protected override void OnCreate()
    {
        World.GetExistingSystem<FixedStepSimulationSystemGroup>().Timestep = Globals.timestep;

        System.DateTime time = System.DateTime.Now;
        run = "run-normal-central";
        path = "Assets/results/" + run + "-" + time.Day + "_" + time.Hour + "_" + time.Minute + "_" + time.Second + "_" + ".txt";
        enabled = true;
        write = false;
    }

    protected override void OnUpdate()
    {
        if (!enabled)
            return;


        //Debug.Log(Time.ElapsedTime + "  : " + 600 * Globals.timestep);
        if (Time.ElapsedTime > 600 * Globals.timestep)
        {
            if (write)
            {
                StreamWriter wr = new StreamWriter("Assets/results/results.txt", true);
                wr.WriteLine(run + " : " + UIManager.instance.count);
                wr.Close();
            }
            enabled = false;
            return;
        }

        string line = "";

        //writer.WriteLine(line);

        Entities.WithoutBurst().ForEach((ref Translation translation, in Movement movement) =>
        {
            translation.Value += movement.Linear;
            line += translation.Value.x + " " + translation.Value.y + " " + translation.Value.z + " ; ";
        }).Run();

        if (write)
        {
            //string path = "Assets/test.txt";
            StreamWriter writer = new StreamWriter(path, true);
            writer.WriteLine(line);
            writer.Close();
        }
        
    }
}