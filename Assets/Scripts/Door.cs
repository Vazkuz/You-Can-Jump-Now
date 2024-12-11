using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Door : Breakable
{
    //Manejar aqui las animaciones. Utilizando, por ejemplo, OnHit y OnBreak
    protected override void OnBreak(ulong player)
    {
        base.OnBreak(player); // en principio la puerta si se destruye ya no se puede regenerar
        networkObject.Despawn(gameObject);
    }
}
