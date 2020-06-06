using System;
using System.Text;
using System.Runtime;
using UnityEngine;
using System.Collections;
using UnityInjector;
using UnityEngine.SceneManagement;
using Valve.VR;



// まずはコントローラの入力をdebug.logできるかを確認してみる

namespace COM3D2.VRYotogiPositionControler
{
    public class VRYotogiPositionControler : UnityInjector.PluginBase
    {
        private bool IsVRMode = false;
        private bool IsYotogi = false;
        private bool IsSpin = false;
        private bool IsMoveX, IsMoveY = false;
        private bool IsPressedLeft, IsPressedRight = false;


        public OvrMgr.OvrObject.Controller LeftController, RightController;
        public OvrMgr OvrMgr;
        public Transform CameraTransform, BaseCameraTransform, RealCameraTransform;


        public static Vector3 Position;
        public static Vector2 LeftAxis, RightAxis;
        public static Vector3 EulerAngles;
        public string scenename = "sceneYotogi";

        public bool IsReverseMode = false;

        public float speed = 0.05f;
        // これがtrueであったら右（左）トリガー、左（右）移動とする
        public static void Initialize()
        {
            Debug.Log("COM3D2PluginTest is Initialized");
        }

        public void Awake()
        {
            Initialize();
        }
        public void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            if (GameMain.Instance.VRMode)
            {
                IsVRMode = true;                
            }
            if(IsVRMode)
            {
                OvrMgr = GameMain.Instance.OvrMgr;
                if(OvrMgr.ovr_obj.left_controller == null)
                {
                    Debug.Log("null de");
                }
                LeftController = OvrMgr.ovr_obj.left_controller;
                RightController = OvrMgr.ovr_obj.right_controller;
            }
            /*
            if (ovrController == null && IsVRMode)
            {
                Debug.Log("ovrcontrolelr is null");
                ovrController = gameObject.AddComponent<OvrControllerButtons>() ??
                                gameObject.GetComponent<OvrControllerButtons>();
                aVRControllerButtons = gameObject.AddComponent<AVRControllerButtons>();
                VRHeightOffset = gameObject.AddComponent<VRHeightOffset>();
            }
            */
        }

        public void Update()
        {

            /*
            EulerAngles = OvrMgr.OvrCamera.GetBaseHeadTransform().eulerAngles;
            EulerAngles = new Vector3(EulerAngles.x + 1, EulerAngles.y + 1, EulerAngles.z + 1);
            OvrMgr.OvrCamera.SetRotation(EulerAngles);
            Debug.Log(EulerAngles);
            */
            // Debug.Log(OvrMgr.OvrCamera.GetBaseHeadTransform().rotation);
            // Debug.Log(OvrMgr.OvrCamera.GetRealHeadTransform().rotation);
            // Debug.Log("   ");

            /*
            Position = OvrMgr.OvrCamera.GetBaseHeadTransform().position;
            Position = new Vector3(Position.x + 00.5f, Position.y, Position.z);
            OvrMgr.OvrCamera.SetPos(Position);
            */
            
            if (!IsVRMode || !IsYotogi)
            {
                Debug.Log(OvrMgr.OvrCamera.GetBaseHeadTransform().transform.position);
                Debug.Log(OvrMgr.OvrCamera.GetRealHeadTransform().transform.position);
                Debug.Log("");
                // 移動したらBaseとRealで同じpositionとなる　
                // ヘッドセットを動かしたときはBaseの値には変化はない
                // 移動後のRealのPosion - 移動前のRealのPosition分をBaseに足してやる
                return;
            }
                
            {
                // Debug.Log(OvrMgr.ovr_obj.left_controller.hand_trans.rotation);
                // var left_hand_trans = OvrMgr.ovr_obj.left_controller.hand_trans;
                // Debug.Log(OvrMgr.ovr_obj.left_controller.controller_buttons.GetTriggerRate());
                // Debug.Log(OvrMgr.OvrCamera.GetRealHeadTransform().position);
                // Debug.Log(OvrMgr.OvrCamera.GetPos());

                // Position = OvrMgr.OvrCamera.GetBaseHeadTransform().position;
                EulerAngles = OvrMgr.OvrCamera.GetBaseHeadTransform().eulerAngles;

                LeftAxis = LeftController.controller_buttons.GetAxis();
                RightAxis = RightController.controller_buttons.GetAxis();

                IsPressedLeft = LeftController.controller_buttons.GetPress(AVRControllerButtons.BTN.VIRTUAL_L_CLICK) ||
                                LeftController.controller_buttons.GetPress(AVRControllerButtons.BTN.VIRTUAL_R_CLICK);
                IsPressedRight = RightController.controller_buttons.GetPress(AVRControllerButtons.BTN.VIRTUAL_L_CLICK) ||
                                 RightController.controller_buttons.GetPress(AVRControllerButtons.BTN.VIRTUAL_R_CLICK);

                CameraTransform = OvrMgr.OvrCamera.GetBaseHeadTransform();
                RealCameraTransform = OvrMgr.OvrCamera.GetRealHeadTransform();

                Debug.Log(OvrMgr.OvrCamera.GetBaseHeadTransform().transform.position);
                Debug.Log(OvrMgr.OvrCamera.GetRealHeadTransform().transform.position);
                Debug.Log("");
                // BaseCameraTransform = OvrMgr.OvrCamera.GetBaseHeadTransform();
                // Transform RealTransform = OvrMgr.OvrCamera.GetRealHeadTransform();
                // Vector3 RealEulerAngles = new Vector3(0, RealTransform.eulerAngles.y, 0);
                // OvrMgr.OvrCamera.SetRotation(RealEulerAngles);
                // EulerAngles = OvrMgr.OvrCamera.GetBaseHeadTransform().eulerAngles;
                // Debug.Log(EulerAngles + " " + OvrMgr.OvrCamera.GetRealHeadTransform().eulerAngles) ;
                // 左（右）のコントローラのトリガーを握ったまま、右（左）のコントローラのボタンで移動させる
                if (LeftController.controller_buttons.GetTriggerRate() == 1)
                {
                    /*
                    if (Math.Abs(RightAxis.x) < 0.7 && IsPressedRight)
                        IsMove = true;
                    else if (Math.Abs(RightAxis.x) >= 0.7 && IsPressedRight && IsMove)
                        IsMove = true;
                    else
                        IsMove = false;
                    */
                    // IsMoveX = IsContinueMove(RightAxis.x, IsPressedRight, IsMoveX);
                    // IsMoveY = IsContinueMove(RightAxis.y, IsPressedRight, IsMoveY);
                    IsMoveX = IsContinueMove(IsReverseMode ? RightAxis.x : LeftAxis.x, 
                                             IsReverseMode ? IsPressedRight : IsPressedLeft, 
                                             IsMoveX);
                    IsMoveY = IsContinueMove(IsReverseMode ? RightAxis.y : LeftAxis.y,
                                             IsReverseMode ? IsPressedRight : IsPressedLeft,
                                             IsMoveY);
                    if(IsMoveX && IsMoveY)
                    {
                            Move(IsReverseMode ? RightAxis : LeftAxis);
                    }
                    // 移動状態でなく、タッチパッドが押されているのであればspin状態と断定できる
                    else if(IsReverseMode ? IsPressedRight : IsPressedLeft)
                    {
                        Spin(IsReverseMode ? RightAxis : LeftAxis);
                    }
                    // Debug.Log(IsMove);
                    
                    // Position = new Vector3(Position.x += 0.1f, Position.y += 0.1f, Position.z += 0.1f);

                }
                if(RightController.controller_buttons.GetTriggerRate() == 1)
                {
                    // IsMoveX = IsContinueMove(LeftAxis.x, IsPressedLeft, IsMoveX);
                    // IsMoveY = IsContinueMove(LeftAxis.y, IsPressedLeft, IsMoveY);
                    // Debug.Log(IsMoveX && IsMoveY);
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
                // (回転時の動作）GetAxis()で得た値の絶対値が閾値を上回った状態でボタンが押されたら回転動作（回転するフラグを立てる）
                // (移動時の動作) GetAxis()で得た値の絶対値が閾値を下回った状態でボタンが押されたら移動動作（移動するフラグを立てる）
                // Debug.Log(LeftController.controller_buttons.GetPress(AVRControllerButtons.BTN.VIRTUAL_L_CLICK));
                // Debug.Log(LeftController.controller_buttons.GetAxis());
                // Move(LeftController.controller_buttons.GetAxis());
                // Debug.Log(Position);
                // OvrMgr.OvrCamera.SetRealHeadPos(Position);これが最後にやってたやつ
                // SetRotation で回転させるんだったらSetPos でベースの座標を現在の座標に書き換えなければいけないかも？
                // OvrMgr.OvrCamera.SetRotation(EulerAngles);
                // OvrMgr.OvrCamera.SetRealHeadRot(EulerAngles);
                // Debug.Log(LeftController.controller_buttons.GetTouchDown(AVRControllerButtons.TOUCH.STICK_PAD));
            }
            
            
            // left_hand_trans.gameObject.AddComponent<AVRControllerButtons>();
            
            /*
            if(ovrController == null && IsVRMode)
            {
                Debug.Log("akan");
            }
            else if(IsVRMode)
            {
                Debug.Log(ovrController.GetTriggerRate());
            }
            */
        }

        public void OnSceneLoaded(Scene scenename, LoadSceneMode scenemode)
        {
            if (GameMain.Instance.GetNowSceneName() == "SceneYotogi")
            {
                IsYotogi = true;
            }
            else IsYotogi = false;
        }

        public void Move(Vector2 Axis)
        {

            Vector3 B_Position = RealCameraTransform.transform.position;
            RealCameraTransform.transform.Translate(transform.forward * Axis.y * speed + transform.right * Axis.x * speed);
            

            // Debug.Log(RealCameraTransform.transform.position);
           
            Vector3 Dif = (RealCameraTransform.transform.position -  B_Position);
            // Debug.Log(Dif);
            // EulerAngles = new Vector3(0, RealHeadTransform.transform.eulerAngles.y, 0);
            // OvrMgr.OvrCamera.SetRotation(EulerAngles);
            // RealHeadTransform.transform.Translate(RealHeadTransform.transform.forward * Axis.y * speed + RealHeadTransform.transform.right * Axis.x * speed);
            //RealHeadTransform.transform.Translate(transform.forward * Axis.y * 0.00005f + transform.right * Axis.x * 0.00005f);
            Position = CameraTransform.transform.position + Dif;           
            // Position = RealHeadTransform.transform.position;
            // Debug.Log(Position);
            // OvrMgr.OvrCamera.SetRotation(EulerAngles);
            // Position = new Vector3(Position.x - Axis.x * speedf, Position.y, Position.z - Axis.y * speedf);
            // Position -= transform.forward * Axis.y * 0.1f;
            // Position -= transform.right * Axis.x * 0.1f;
            // OvrMgr.OvrCamera.SetPos(Position);

            OvrMgr.OvrCamera.SetPos(Position);

        }
        public  void Spin(Vector2 Axis)
        {
            if (Axis.x >= 0.70 || Axis.x <= -0.70)
            {
                // CameraTransform.transform.Rotate(transform.up, 1);
                // EulerAngles = new Vector3(EulerAngles.x, EulerAngles.y + Axis.x, EulerAngles.z);
                // CameraTransforms.transform.RotateAround(new Vector3(0, 1, 0), CameraTransform.position, 1);
                // Debug.Log(CameraTransforms.eulerAngles);
                // OvrMgr.OvrCamera.SetRotation(CameraTransform.eulerAngles);
                EulerAngles = new Vector3(0, EulerAngles.y + Axis.x, 0);
                
                
                // Debug.Log(CameraTransform.eulerAngles);
                // Debug.Log("FF");
                OvrMgr.OvrCamera.SetRotation(EulerAngles);

            }

            else if (Axis.y >= 0.70 || Axis.y <= -0.70)
            {
                // Position = new Vector3(Position.x, Position.y + Axis.y, Position.z);
                CameraTransform.transform.Translate(transform.up * Axis.y * speed);
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
