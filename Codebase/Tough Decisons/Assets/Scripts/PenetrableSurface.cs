using UnityEngine;

public class PenetrableSurface : MonoBehaviour
{
    
    private enum SurfacePhysicalMaterial { NOTHING, WOOD, CONCRETE, STEEL, CUSTOM }
    
    public struct WallPenetrationData
    {
        public readonly RaycastHit raycastHit;
        public readonly float thickness;

        public WallPenetrationData(RaycastHit raycastHit, float thickness)
        {
            this.raycastHit = raycastHit;
            this.thickness = thickness;
        }
    }
    
    [SerializeField] private SurfacePhysicalMaterial surfaceMaterialType = SurfacePhysicalMaterial.CONCRETE;
    [SerializeField] private float customMaterialDensity = 5f;

    public float GetMaterialDensity()
    {
        switch (surfaceMaterialType)
        {
            case SurfacePhysicalMaterial.WOOD:
                return 10f;
            
            case SurfacePhysicalMaterial.CONCRETE:
                return 18f;
            
            case SurfacePhysicalMaterial.STEEL:
                return 25f;
            
            case SurfacePhysicalMaterial.CUSTOM:
                return customMaterialDensity;

            default:
                return 0;
        }
    }
}
