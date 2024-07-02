using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class TankTrackImprintSpawner : NetworkBehaviour
{

    [SerializeField]
    GameObject tankTrackImprintPrefab;

    [SerializeField]
    float minimumSpawnDistance = 3f;

    [SerializeField]
    Transform tankTreads;

    private Vector3 previousSpawnPoint;

    private bool hasSpawnedFirst = false;

    // Update is called once per frame
    void Update()
    {
        if (!hasSpawnedFirst)
        {
            SpawnTankTrackImprint();
            hasSpawnedFirst = true;
            previousSpawnPoint = this.transform.position;
            return;
        }
        if(Vector3.Distance(previousSpawnPoint, this.transform.position) >= minimumSpawnDistance)
        {
            SpawnTankTrackImprint();
            previousSpawnPoint = this.transform.position;
        }
    }

    private void SpawnTankTrackImprint()
    {
        Instantiate(tankTrackImprintPrefab, this.transform.position, tankTreads.rotation);
    }
}
