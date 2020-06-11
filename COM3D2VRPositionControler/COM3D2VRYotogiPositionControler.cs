using System;
using System.Text;
using System.Runtime;
using UnityEngine;
using System.Collections;
using UnityInjector;
using UnityEngine.SceneManagement;
using Valve.VR;
using System.IO;
using COM3D2VRPositionControler;
using UnityInjector.Attributes;



// まずはコントローラの入力をdebug.logできるかを確認してみる

namespace COM3D2.VRYotogiPositionControler
{
    [PluginFilter("COM3D2x64")]
    [PluginFilter("COM3D2VRx64")]
    [PluginName("COM3D2.VRYotogiPositionControler.Plugin")]
    [PluginVersion("1.0.0.0")]

    public class VRYotogiPositionControler : UnityInjector.PluginBase
    {
        private bool IsVRMode = false;
        private bool IsYotogi = false;
        private bool IsMoveX, IsMoveY = false;
        private bool IsPressedLeft, IsPressedRight = false;
        private Vector3 Dif;

        private OvrMgr.OvrObject.Controller LeftController, RightController;
        private OvrMgr OvrMgr;
        private Transform CameraTransform, RealCameraTransform;


        private static Vector3 Position;
        private static Vector2 LeftAxis, RightAxis;
        private static Vector3 EulerAngles;
        private static Vector3 B_Position;

        private bool IsReverseMode = Settings.IsReverseMode;
        private bool IsAbsoluteMoveMode = Settings.IsAbsoluteMoveMode;
        private float MoveSpeed = Settings.MoveSpeed;
        private float UpSpeed = Settings.UpSpeed;
        private float SpinSpeed = Settings.SpinSpeed;
        
        // これがtrueであったら右（左）トリガー、左（右）移動とする

        public void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            if (GameMain.Instance.VRMode)
            {
                IsVRMode = true;
            }
            if (IsVRMode)
            {
                OvrMgr = GameMain.Instance.OvrMgr;
                LeftController = OvrMgr.ovr_obj.left_controller;
                RightController = OvrMgr.ovr_obj.right_controller;
            }
            if(Settings.IsFolderExist())
            // 設定ファイルが読み込めたら
            {
                // ここで設定を再格納
                IsReverseMode = Settings.IsReverseMode;
                IsAbsoluteMoveMode = Settings.IsAbsoluteMoveMode;
                MoveSpeed = Settings.MoveSpeed;
                UpSpeed = Settings.UpSpeed;
                SpinSpeed = Settings.SpinSpeed;
            }
            Debug.Log(IsReverseMode);
            Debug.Log(IsAbsoluteMoveMode);
            Debug.Log(MoveSpeed);
            Debug.Log(UpSpeed);
            Debug.Log(SpinSpeed);
        }

        public void Update()
        {
            if (!IsVRMode || !IsYotogi)
                return;


            EulerAngles = OvrMgr.OvrCamera.GetBaseHeadTransform().eulerAngles;

            LeftAxis = LeftController.controller_buttons.GetAxis();
            RightAxis = RightController.controller_buttons.GetAxis();

            IsPressedLeft = LeftController.controller_buttons.GetPress(AVRControllerButtons.BTN.VIRTUAL_L_CLICK) ||
                            LeftController.controller_buttons.GetPress(AVRControllerButtons.BTN.VIRTUAL_R_CLICK);
            IsPressedRight = RightController.controller_buttons.GetPress(AVRControllerButtons.BTN.VIRTUAL_L_CLICK) ||
                             RightController.controller_buttons.GetPress(AVRControllerButtons.BTN.VIRTUAL_R_CLICK);

            CameraTransform = OvrMgr.OvrCamera.GetBaseHeadTransform();


            // 左（右）のコントローラのトリガーを握ったまま、右（左）のコントローラのボタンで移動させる(IsReverseMode = trueなら)
            if (LeftController.controller_buttons.GetTriggerRate() == 1)
            {
                IsMoveX = IsContinueMove(IsReverseMode ? RightAxis.x : LeftAxis.x,
                                         IsReverseMode ? IsPressedRight : IsPressedLeft,
                                         IsMoveX);
                IsMoveY = IsContinueMove(IsReverseMode ? RightAxis.y : LeftAxis.y,
                                         IsReverseMode ? IsPressedRight : IsPressedLeft,
                                         IsMoveY);
                if (IsMoveX && IsMoveY)
                {
                    Move(IsReverseMode ? RightAxis : LeftAxis);
                }
                // 移動状態でなく、タッチパッドが押されているのであればspin状態と断定できる
                else if (IsReverseMode ? IsPressedRight : IsPressedLeft)
                {
                    Spin(IsReverseMode ? RightAxis : LeftAxis);
                }
            }

            if (RightController.controller_buttons.GetTriggerRate() == 1)
            {
                IsMoveX = IsContinueMove(IsReverseMode ? LeftAxis.x : RightAxis.x,
                                         IsReverseMode ? IsPressedLeft : IsPressedRight,
                                         IsMoveX);
                IsMoveY = IsContinueMove(IsReverseMode ? LeftAxis.y : RightAxis.y,
                                         IsReverseMode ? IsPressedLeft : IsPressedRight,
                                         IsMoveY);
                if (IsMoveX && IsMoveY)
                {
                    Move(IsReverseMode ? LeftAxis : RightAxis);
                }
                else if (IsReverseMode ? IsPressedLeft : IsPressedRight)
                {
                    Spin(IsReverseMode ? LeftAxis : RightAxis);
                }

            }

        }

        public void OnSceneLoaded(Scene scenename, LoadSceneMode scenemode)
        {
            if (GameMain.Instance.GetNowSceneName() == "SceneYotogi" && !IsYotogi)
            {
                IsYotogi = true;
                // Pre_BasePosition = OvrMgr.OvrCamera.GetBaseHeadTransform().position;
                // Pre_HeadPosition = OvrMgr.OvrCamera.GetRealHeadTransform().position;

            }
            else if (IsYotogi)
            // 夜伽終了時に実行
            {
                IsYotogi = false;
                // OvrMgr.OvrCamera.SetPos(Pre_BasePosition);
                // OvrMgr.OvrCamera.SetRealHeadPos(Pre_HeadPosition);
            }


            // Debug.Log(GameMain.Instance.GetNowSceneName() + OvrMgr.OvrCamera.GetBaseHeadTransform().transform.position);
        }


        public void Move(Vector2 Axis)
        {
            // 移動したらBaseとRealで同じpositionとなる　
            // ヘッドセットを動かしたときはBaseの値には変化はない
            // 移動後のRealのPosion - 移動前のRealのPosition分をBaseに足してやる
            if (IsAbsoluteMoveMode)
            {
                CameraTransform.transform.Translate(transform.forward * Axis.y * MoveSpeed + transform.right * Axis.x * MoveSpeed);
                Position = CameraTransform.transform.position;
                OvrMgr.OvrCamera.SetPos(Position);
            }
            else if(!IsAbsoluteMoveMode)
            {
                RealCameraTransform = OvrMgr.OvrCamera.GetRealHeadTransform();
                B_Position = RealCameraTransform.transform.position;
                RealCameraTransform.transform.Translate(transform.forward * Axis.y * MoveSpeed + transform.right * Axis.x * MoveSpeed);
                Dif = RealCameraTransform.transform.position - B_Position;            
                Position = CameraTransform.transform.position + Dif;
                RealCameraTransform.transform.Translate(-transform.forward * Axis.y * MoveSpeed - transform.right * Axis.x * MoveSpeed);
                OvrMgr.OvrCamera.SetPos(Position);
            }
            

        }
        public void Spin(Vector2 Axis)
        {
            if (Axis.x >= 0.70 || Axis.x <= -0.70)
            {
                EulerAngles = new Vector3(0, EulerAngles.y + (Axis.x * SpinSpeed), 0);
                OvrMgr.OvrCamera.SetRotation(EulerAngles);

            }

            else if (Axis.y >= 0.70 || Axis.y <= -0.70)
            {
                CameraTransform.transform.Translate(transform.up * Axis.y * MoveSpeed * UpSpeed);
                Position = CameraTransform.transform.position;
                OvrMgr.OvrCamera.SetPos(Position);
            }


        }

        public bool IsContinueMove(float Axis, bool IsPressed, bool IsMove)
        // 引数に与えるのは、左（右）のコントローラの軸入力と左（右）が押されているかどうかのbool
        {
            if (Math.Abs(Axis) < 0.70 && IsPressed)
                return true;
            else if ((Math.Abs(Axis) >= 0.70 && IsPressed && IsMove))
                return true;
            else
                return false;
        }
    }
}
