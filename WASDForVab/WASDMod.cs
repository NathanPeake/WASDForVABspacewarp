﻿using System;
using BepInEx;
using I2.Loc;
using KSP.Game;
using KSP.Modules;
using KSP.OAB;
using KSP.Sim;
using UnityEngine;
using HarmonyLib;
using KSP.Messages;
using System.Reflection;

namespace KSPTestMod
{
    [BepInPlugin("io.archlinus.wasd_for_vab", "WASD for VAB", "0.1.2")]
    public class WASDMod : BaseUnityPlugin
    {
        private GameInstance Game;

        private Harmony harmony;

        private SubscriptionHandle loadSubscription;

        public void Start()
        {
            var assembly = Assembly.GetExecutingAssembly();
            harmony = Harmony.CreateAndPatchAll(assembly);
        }

        private void OnOABLoadFinalized(MessageCenterMessage msg)
        {
            if (Game != null && Game.OAB != null && Game.OAB.Current != null)
            {
                ObjectAssemblyBuilder current = Game.OAB.Current;
                WASDPatches.patchState.OnLoad(current.CameraManager, Logger);
            }
        }

        public void Update()
        {
            if (Game == null && GameManager.Instance != null && GameManager.Instance.Game != null)
            {
                Game = GameManager.Instance.Game;
                loadSubscription = Game.Messages.Subscribe<OABLoadFinalizedMessage>(OnOABLoadFinalized);
            }

            WASDPatches.patchState.isCtrlPressed = false;
            if (Game != null)
            {
                if (Game.OAB != null && Game.OAB.IsLoaded)
                {
                    // Toggle WASD cam on ALT+w
                    if (Input.GetKey(KeyCode.LeftAlt))
                    {
                        if (Input.GetKeyDown(KeyCode.W))
                        {
                            WASDPatches.patchState.isEnabled = !WASDPatches.patchState.isEnabled;
                        }
                    }

                    if (Input.GetMouseButton(1))
                    {
                        Vector3d inputVector = new Vector3d();
                        if (Input.GetKey(KeyCode.W))
                        {
                            inputVector.z = 1;
                        }
                        if (Input.GetKey(KeyCode.S))
                        {
                            inputVector.z = -1;
                        }
                        if (Input.GetKey(KeyCode.D))
                        {
                            inputVector.x = 1;
                        }
                        if (Input.GetKey(KeyCode.A))
                        {
                            inputVector.x = -1;
                        }
                        if (Input.GetKey(KeyCode.E))
                        {
                            inputVector.y = 1;
                        }
                        if (Input.GetKey(KeyCode.Q))
                        {
                            inputVector.y = -1;
                        }

                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            inputVector *= 2.0f;
                        }

                        if (Input.GetKey(KeyCode.LeftControl))
                        {
                            inputVector *= 0.5f;
                            WASDPatches.patchState.isCtrlPressed = true;
                        }

                        if (!inputVector.IsZero())
                        {
                            WASDPatches.patchState.OnMove(inputVector, Time.deltaTime);
                        }
                    }
                }
            }
        }

        public void OnDestroy()
        {
            if (Game != null)
            {
                Game.Messages.Unsubscribe(ref loadSubscription);
            }

            harmony.UnpatchSelf();
        }

    }
}
