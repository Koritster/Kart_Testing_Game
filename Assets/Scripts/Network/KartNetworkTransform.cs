using Unity.Netcode.Components;
using UnityEngine;

public class KartNetworkTransform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return true;
    }
}
