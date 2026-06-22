public static class VoxelID
{
    // ==========================================
    // 1. NATURE & BIOMES (IDs 0-9)
    // ==========================================
    public const byte WATER = 0;          // Air (Water)
    public const byte GRASS = 1;          // Tanah/Rumput (Grass)
    public const byte CONCRETE = 2;       // Beton (Concrete/Asphalt)
    public const byte ROUGH_GROUND = 3;   // Batu/Tanah Kasar (Stone/Rough Ground)

    // ==========================================
    // 2. INFRASTRUCTURE & SKELETON (IDs 10-29)
    // ==========================================
    public const byte ROAD = 10;          // Jalan Utama / Lokal
    public const byte SEWER = 11;         // Selokan / Gutter
    public const byte BRIDGE = 12;        // Jembatan

    // ==========================================
    // 3. ZONING & CANDIDATES (IDs 30-39)
    // ==========================================
    public const byte ZONE_RESIDENTIAL = 30; // Zona Perumahan (Transparent Green Floor)
    public const byte ZONE_INDUSTRIAL = 31;  // Zona Industri (Transparent Yellow Floor)
    public const byte ZONE_AGRICULTURAL = 32; // Zona Pertanian
    public const byte ZONE_WATER_GREEN = 33;  // Zona Perairan Hijau (Biopori/Resapan)

    // ==========================================
    // 4. COMPLETED BUILDINGS / WFC (IDs 40+)
    // ==========================================
    public const byte BUILDING_HOUSE = 40;   // Rumah Kecil (Residential Building)
    public const byte BUILDING_FACTORY = 41; // Pabrik (Industrial Building)
}

public static class VoxelHelper
{
    public static void InitializeHydrology(ref VoxelCell cell, byte blockType)
    {
        cell.blockType = blockType;
        switch (blockType)
        {
            case VoxelID.WATER:
                cell.zoneType = ZoneType.WATER_BODY;
                cell.absorptionRate = 0.0f;
                cell.flowFriction = 0.0f;
                cell.isPollutionSource = false;
                break;
            case VoxelID.GRASS:
                cell.zoneType = ZoneType.EMPTY;
                cell.absorptionRate = 0.15f;
                cell.flowFriction = 0.7f;
                cell.isPollutionSource = false;
                break;
            case VoxelID.CONCRETE:
                cell.zoneType = ZoneType.EMPTY;
                cell.absorptionRate = 0.0f;
                cell.flowFriction = 0.1f;
                cell.isPollutionSource = false;
                break;
            case VoxelID.ROUGH_GROUND:
                cell.zoneType = ZoneType.EMPTY;
                cell.absorptionRate = 0.05f;
                cell.flowFriction = 0.5f;
                cell.isPollutionSource = false;
                break;
            case VoxelID.ROAD:
                cell.zoneType = ZoneType.EMPTY;
                cell.absorptionRate = 0.0f;
                cell.flowFriction = 0.1f;
                cell.isPollutionSource = false;
                break;
            case VoxelID.BRIDGE:
                cell.zoneType = ZoneType.EMPTY;
                cell.absorptionRate = 0.0f;
                cell.flowFriction = 0.1f;
                cell.isPollutionSource = false;
                break;
            case VoxelID.ZONE_RESIDENTIAL:
                cell.zoneType = ZoneType.RESIDENTIAL;
                cell.absorptionRate = 0.05f;
                cell.flowFriction = 0.5f;
                cell.isPollutionSource = false;
                break;
            case VoxelID.ZONE_INDUSTRIAL:
                cell.zoneType = ZoneType.INDUSTRIAL;
                cell.absorptionRate = 0.01f;
                cell.flowFriction = 0.2f;
                cell.isPollutionSource = true;
                break;
            case VoxelID.ZONE_AGRICULTURAL:
                cell.zoneType = ZoneType.AGRICULTURAL;
                cell.absorptionRate = 0.2f;
                cell.flowFriction = 0.8f;
                cell.isPollutionSource = false;
                break;
            case VoxelID.ZONE_WATER_GREEN:
                cell.zoneType = ZoneType.WATER_GREEN;
                cell.absorptionRate = 0.3f; // Daya resapan sangat tinggi
                cell.flowFriction = 0.9f; // Menghambat limpasan air sangat tinggi
                cell.isPollutionSource = false;
                break;
            case VoxelID.BUILDING_HOUSE:
                cell.zoneType = ZoneType.RESIDENTIAL;
                cell.absorptionRate = 0.02f;
                cell.flowFriction = 0.6f;
                cell.isPollutionSource = false;
                break;
            case VoxelID.BUILDING_FACTORY:
                cell.zoneType = ZoneType.INDUSTRIAL;
                cell.absorptionRate = 0.0f;
                cell.flowFriction = 0.2f;
                cell.isPollutionSource = true;
                break;
            default:
                cell.zoneType = ZoneType.EMPTY;
                cell.absorptionRate = 0.05f;
                cell.flowFriction = 0.5f;
                cell.isPollutionSource = false;
                break;
        }
    }
}
