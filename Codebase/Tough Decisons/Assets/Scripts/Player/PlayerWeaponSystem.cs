using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine.Profiling;

public class PlayerWeaponSystem : MonoBehaviour
{

    [Header("Damage Settings")]
    [SerializeField] private float maxDamage = 25f;
    [SerializeField] private float minDamage = 15f;
    [SerializeField] private AnimationCurve damageCurve = AnimationCurve.Linear(0, 1, 1, 0);

    [Header("Spread Settings")]
    [SerializeField] private float baseSpread = .05f;
    [SerializeField] private float maxSpread = .35f;
    [SerializeField] private float spreadAmplitude = 0.05f;
    [SerializeField] private AnimationCurve spreadCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Ammo Settings")]
    [SerializeField] private int bulletsPerShot = 1;
    [SerializeField] private int magazineSize = 25;
    [SerializeField] private int spawnAmmo = 150;
    
    [Header("Penetration Settings")]
    [SerializeField] private float penetrationForce = 25f;
    [SerializeField] private AnimationCurve penetrationDamageReductionCurve = AnimationCurve.Linear(0, 1, 1, 0);
    
    [Header("Additional Settings")]
    [SerializeField] private float fireRange = 100f;
    [SerializeField] private int fireRate = 10;
    
    // Components
    private Camera cam;

    private bool roundInChamber; // If a bullet is in the chamber
    
    private float currentSpread; // The area spread of hipfire
    private float nextShot; // When next shot can be fired

    // Ammo data
    private int magazineCount; // Ammo in the gun (not including in chamber)
    private int ammoCount; // Ammo outside of gun

    private List<PenetrableSurface> penetrableSurfaces; // Surfaces that bullets can travel through (caching)

    void Awake()
    {
        cam = Camera.main;

        roundInChamber = true;
        currentSpread = baseSpread;
        magazineCount = magazineSize;
        ammoCount = (spawnAmmo - magazineSize - 1);
        nextShot = Time.time;
        
        penetrableSurfaces = new List<PenetrableSurface>();
    }
    
    void Update()
    {
        if (Input.GetButton("Fire1"))
            FireWeaponSystem();
        
        if (currentSpread > baseSpread)
        {
            float spreadRange = Mathf.InverseLerp(baseSpread, maxSpread, currentSpread);
            spreadRange -= spreadAmplitude * Time.deltaTime;

            currentSpread = spreadCurve.Evaluate(spreadRange) * (maxSpread - baseSpread) + baseSpread;
        }
    }

    public void FireWeaponSystem()
    {
        if (!CanFireWeaponSystem())
            return;
        
        for (int i = 0; i < bulletsPerShot; ++i)
        {
            Vector2 randUnitCircle = Random.insideUnitCircle;
            
            Vector3 spread = ((randUnitCircle.x * currentSpread) * cam.transform.right) + ((randUnitCircle.y * currentSpread) * cam.transform.up);
            Vector3 rayDirection = (cam.transform.forward + spread).normalized;

            RaycastHit firstHit;
            if (Physics.Raycast(cam.transform.position, rayDirection, out firstHit, fireRange))
            {
                Debug.DrawLine(cam.transform.position, firstHit.point, Color.red);
                // TODO: Play VFX Here

                PenetrableSurface hitSurface = GetSurfaceFromRaycastHit(firstHit);
                RaycastHit lastHit = firstHit;
                
                float penetrationAmount = 0;
                
                while (hitSurface != null)
                {
                    PenetrableSurface.WallPenetrationData wallData = CalculateWallData(lastHit, rayDirection);
                    if (wallData.thickness <= 0)
                        break;

                    // TODO: Play VFX Here

                    penetrationAmount += hitSurface.GetMaterialDensity() * wallData.thickness;
                    if (penetrationAmount > penetrationForce)
                        break;
                    
                    lastHit = wallData.raycastHit;

                    float distanceLeft = fireRange - Vector3.Distance(firstHit.point, lastHit.point);
                    if (distanceLeft <= 0)
                        break;

                    Vector3 debug = lastHit.point;
                    if (Physics.Raycast(lastHit.point, rayDirection, out lastHit, distanceLeft))
                    {
                        Debug.DrawLine(debug, lastHit.point, Color.red);
                        hitSurface = GetSurfaceFromRaycastHit(lastHit);

                        // TODO: Play VFX Here
                    }

                    else
                    {
                        Debug.DrawRay(debug, rayDirection * distanceLeft, Color.red);
                        break;
                    }
                }
            }
            
            else
                Debug.DrawRay(cam.transform.position, rayDirection * fireRange, Color.red);
        }

        nextShot = Time.time + (1.0f / fireRate);
        currentSpread += spreadAmplitude;

//        if (--magazineCount == 0)
//            roundInChamber = false;

        // TODO: Play SFX Here
    }

    PenetrableSurface GetSurfaceFromRaycastHit(RaycastHit surfaceHit)
    {
        foreach (PenetrableSurface surface in penetrableSurfaces)
            if (surface.transform == surfaceHit.transform)
                return surface;
        
        PenetrableSurface foundSurface = surfaceHit.transform.GetComponent<PenetrableSurface>();
        
        if (foundSurface != null)
            penetrableSurfaces.Add(foundSurface);

        return foundSurface;
    }

    float CalculateWeaponDamage(float distance, float penetrationAmount)
    {
        if (penetrationAmount > penetrationForce)
            return 0;

        float distanceBasedDamage = damageCurve.Evaluate(distance / fireRange) * (maxDamage - minDamage);
        distanceBasedDamage += minDamage;
        
        return distanceBasedDamage * penetrationDamageReductionCurve.Evaluate(penetrationAmount / penetrationForce);
    }

    PenetrableSurface.WallPenetrationData CalculateWallData(RaycastHit hit, Vector3 rayDirection)
    {
        Vector3 reverseRayOrigin = hit.point + (rayDirection * 5f);

        RaycastHit backsideHit;
        if (hit.collider.Raycast(new Ray(reverseRayOrigin, -rayDirection), out backsideHit, 7f))
        {
            Debug.DrawLine(hit.point, backsideHit.point, Color.green);
            return new PenetrableSurface.WallPenetrationData(backsideHit, Vector3.Distance(hit.point, backsideHit.point));
        }
        
        return new PenetrableSurface.WallPenetrationData(backsideHit, 0);
    }

    bool CanFireWeaponSystem()
    {
        return (magazineCount < 0 || roundInChamber) && nextShot <= Time.time;
    }
}
