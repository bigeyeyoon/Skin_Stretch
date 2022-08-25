using AirSimUnity.CarStructs;
using UnityEngine;
using UnityEngine.AI;

namespace AirSimUnity
{
    /*
     * Car component that is used to control the car object in the scene. A sub class of Vehicle that is communicating with AirLib.
     * This class depends on the AirSimController component to move the car based on Unity physics.
     * The car can be controlled either through keyboard or through client api calls.
     * This class holds the current car state and data for client to query at any point of time.
     */
    [RequireComponent(typeof(AirSimCarController))]
    //22.08.17 나영 추가
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(LineRenderer))]
    //

    public class Car : Vehicle
    {
        private AirSimCarController carController;


        //22.08.17 나영 추가
        private NavMeshAgent agent;
        private LineRenderer lineRenderer;

        private NavMeshPath path;
        private Vector3 targetPosition;
        //

        private CarControls carControls;
        private CarState carState;
        private CarData carData;

        private float steering, throttle, footBreak, handBrake;

        private Rigidbody rigidBody;

        private void Awake()
        {
            rigidBody = GetComponent(type: typeof(Rigidbody)) as Rigidbody;

        }

        private new void Start()
        {
            base.Start();
            carController = GetComponent<AirSimCarController>();

            //22.08.17 나영 추가
            agent = GetComponent<NavMeshAgent>();
            lineRenderer = GetComponent<LineRenderer>();

            lineRenderer.startWidth = 0.15f;
            lineRenderer.endWidth = 0.15f;
            lineRenderer.positionCount = 0;

            agent = GetComponent<NavMeshAgent>();

            path = new NavMeshPath();
            //

        }

        private new void FixedUpdate()
        {
            if (isServerStarted)
            {
                if (resetVehicle)
                {
                    resetVehicle = false;
                    carData.Reset();
                    carControls.Reset();
                    rcData.Reset();
                    DataManager.SetToUnity(poseFromAirLib.position, ref position);
                    DataManager.SetToUnity(poseFromAirLib.orientation, ref rotation);
                    transform.position = position;
                    transform.rotation = rotation;
                    currentPose = poseFromAirLib;
                    steering = 0;
                    throttle = 0;
                    footBreak = 0;
                    handBrake = 0;

                    var rb = GetComponent<Rigidbody>();
                    rb.constraints = RigidbodyConstraints.FreezeAll;
                    rb.constraints = RigidbodyConstraints.None;
                }
                else
                {
                    base.FixedUpdate();

                    if (isApiEnabled)
                    {
                        throttle = carControls.throttle;
                        handBrake = carControls.handbrake ? 1 : 0;
                        footBreak = carControls.brake;
                        steering = carControls.steering;
                    }
                    else
                    {
                        //22.08.17 나영 추가

                        

                        if (Input.GetMouseButtonDown(0))
                        {
                            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                            RaycastHit hit;

                            if (Physics.Raycast(ray, out hit))
                            {
                                targetPosition = hit.point;
                                agent.CalculatePath(targetPosition, path);
                            }

                            Debug.Log(targetPosition);
                        }

                        agent.CalculatePath(targetPosition, path);

                        if (path.corners.Length > 1)
                            DrawPath();

                        //


                        steering = Input.GetAxis("Horizontal");
                        throttle = Input.GetAxis("Vertical");
                        handBrake = Input.GetAxis("Jump");
                        footBreak = throttle;
                    }

                    carController.Move(steering, throttle, footBreak, handBrake);
                    carController.UpdateCarData(ref carData);
                    carData.throttle = throttle;
                    carData.brake = footBreak;
                    carData.steering = steering;
                }
            }
        }

        //Car controls being set through client api calls
        public override bool SetCarControls(CarControls controls)
        {
            DataManager.SetCarControls(controls, ref carControls);
            return true;
        }

        //Queried by the client through rpc to AirLib
        public override CarState GetCarState()
        {
            carState.speed = carData.speed;
            carState.gear = carData.gear;
            carState.pose = currentPose;
            carState.timeStamp = DataManager.GetCurrentTimeInMilli();
            carState.engineMaxRotationSpeed = carData.engineMaxRotationSpeed;
            carState.engineRotationSpeed = carData.engineRotationSpeed;
            return carState;
        }

        //Get the current car data to save in a text file along with the images taken while recording.
        public override DataRecorder.ImageData GetRecordingData()
        {
            DataRecorder.ImageData data;
            data.pose = currentPose;
            data.carData = carData;
            data.image = null;
            return data;
        }

        //Override the method to reduce Missing Reference Exception to the Car
        public override AirSimVector GetVelocity()
        {
            return rigidBody != null ? new AirSimVector(rigidBody.velocity.x, rigidBody.velocity.y, rigidBody.velocity.z) : new AirSimVector(0f, 0f, 0f);
        }

        void DrawPath()
        {
            lineRenderer.positionCount = path.corners.Length;
            lineRenderer.SetPosition(0, transform.position);

            if (path.corners.Length < 2)
                return;
            else
            {
                for (int i = 1; i < path.corners.Length; i++)
                {
                    Vector3 pointPosition = new Vector3(path.corners[i].x, path.corners[i].y, path.corners[i].z);
                    lineRenderer.SetPosition(i, pointPosition);
                }
            }
        }

    }
}