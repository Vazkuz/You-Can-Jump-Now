using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Mineral : Breakable
{
    //Manejar aqui las animaciones. Utilizando, por ejemplo, OnHit y OnBreak
    // El mineral, eventualmente, tambien puede regenerarse
    protected override void OnBreak(ulong player)
    {
        base.OnBreak(player); //eventualmente cambiar esto, no se destruye sino que se esconde mientras el jugador no salte o algo asi
        networkObject.Despawn(gameObject);
    }
}
