public static class Layers
{
    public const int Default = 0;
    public const int DefaultMask = 1 << Default;
    public const int TransparentFX = 1;
    public const int TransparentFXMask = 1 << TransparentFX;
    public const int IgnoreRaycast = 2;
    public const int IgnoreRaycastMask = 1 << IgnoreRaycast;
    public const int Water = 4;
    public const int WaterMask = 1 << Water;
    public const int UI = 5;
    public const int UIMask = 1 << UI;

    // Custom layers
    public const int CustomLayer1 = 6;
    public const int CustomLayer1Mask = 1 << CustomLayer1;
    public const int CustomLayer2 = 7;
    public const int CustomLayer2Mask = 1 << CustomLayer2;
    public const int CustomLayer3 = 8;
    public const int CustomLayer3Mask = 1 << CustomLayer3;
    public const int CustomLayer4 = 9;
    public const int CustomLayer4Mask = 1 << CustomLayer4;
    public const int CustomLayer5 = 10;
    public const int CustomLayer5Mask = 1 << CustomLayer5;
    public const int CustomLayer6 = 11;
    public const int CustomLayer6Mask = 1 << CustomLayer6;
    public const int CustomLayer7 = 12;
    public const int CustomLayer7Mask = 1 << CustomLayer7;
    public const int CustomLayer8 = 13;
    public const int CustomLayer8Mask = 1 << CustomLayer8;

    public const int AvatarRemote = 29;
    public const int AvatarRemoteMask = 1 << AvatarRemote;
    public const int AvatarLocal = 30;
    public const int AvatarLocalMask = 1 << AvatarLocal;
    public const int Environment = 31;
    public const int EnvironmentMask = 1 << Environment;
}
