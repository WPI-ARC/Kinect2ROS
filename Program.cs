using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace Kinect2ROS
{
    class Program
    {
        private static bool publish_depth = false;
        private static bool publish_color = false;
        private static bool publish_skeleton = true;
        private static ROS_CS.SocketBridge.SocketTX<ROS_CS.sensor_msgs.Image> color_tx;
        private static ROS_CS.SocketBridge.SocketTX<ROS_CS.sensor_msgs.Image> depth_tx;
        private static ROS_CS.SocketBridge.SocketTX<ROS_CS.person_tracker.TrackerState> skeleton_tx;
        //private static ROS_CS.SocketBridge.SocketRX<ROS_CS.sensor_msgs.Image> color_rx;
        private static KinectSensor kinect;
        private static ColorFrameReader color_reader;
        private static DepthFrameReader depth_reader;
        private static BodyFrameReader skeleton_reader;
        private static CoordinateMapper coordinate_mapper;
        private static TimeSpan unix_base_ts;
        private static readonly long windows_epoch_to_unix_epoch_seconds = 11644473600;
        private static readonly long ticks_per_second = 10000000;
        private static readonly long unix_base_ticks = windows_epoch_to_unix_epoch_seconds * ticks_per_second;

        static void Main(string[] args)
        {
            unix_base_ts = new TimeSpan(unix_base_ticks);
            // Initialize the kinect sensor
            //kinect IS NOT AN INSTANTIABLE OBJECT - you get a 'device' from the system driver
            try
            {
                kinect = KinectSensor.GetDefault();
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("It appears that no Kinect sensor is connected to your computer!");
                return;
            }
            // Data reader device initialization code
            try
            {
                Console.WriteLine("Starting initialization");
                kinect.Open();
                // Init color images
                color_reader = kinect.ColorFrameSource.OpenReader();
                // Init depth images
                depth_reader = kinect.DepthFrameSource.OpenReader();
                // Init skeleton tracking
                skeleton_reader = kinect.BodyFrameSource.OpenReader();
                // Init the coordinate mapper
                coordinate_mapper = kinect.CoordinateMapper;
                Console.WriteLine("Initialization successful");
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Failed trying to set up the kinect. Is the device connected, on, and not being used by another application?");
                return;
            }
            // Initialize the socket bridge
            color_tx = new ROS_CS.SocketBridge.SocketTX<ROS_CS.sensor_msgs.Image>(9001);
            depth_tx = new ROS_CS.SocketBridge.SocketTX<ROS_CS.sensor_msgs.Image>(9002);
            skeleton_tx = new ROS_CS.SocketBridge.SocketTX<ROS_CS.person_tracker.TrackerState>(9003);
            //color_rx = new ROS_CS.SocketBridge.SocketRX<ROS_CS.sensor_msgs.Image>("127.0.0.1", 9001, ColorImageCB);
            bool control = true;
            long depth_timestamp = 0;
            long color_timestamp = 0;
            long skeleton_timestamp = 0;
            while (control)
            {
                if (publish_depth)
                {
                    //Get depth data
                    DepthFrame latest_depth = depth_reader.AcquireLatestFrame();
                    if (latest_depth != null)
                    {
                        using (latest_depth)
                        {
                            Console.WriteLine("Current depth timestamp: {0:D}, new timestamp: {1:D}", depth_timestamp, latest_depth.RelativeTime.Ticks);
                            if (latest_depth.RelativeTime.Ticks > depth_timestamp)
                            {
                                ROS_CS.sensor_msgs.Image depth_image = GetDepthImageFromRaw(latest_depth);
                                if (depth_image != null)
                                {
                                    Console.WriteLine("Sending depth image");
                                    depth_tx.Send(depth_image);
                                }
                                else
                                {
                                    Console.WriteLine("Null depth image");
                                }
                                depth_timestamp = latest_depth.RelativeTime.Ticks;
                            }
                        }
                    }
                }
                if (publish_color)
                {
                    //Get color data
                    ColorFrame latest_color = color_reader.AcquireLatestFrame();
                    if (latest_color != null)
                    {
                        using (latest_color)
                        {
                            Console.WriteLine("Current color timestamp: {0:D}, new timestamp: {1:D}", color_timestamp, latest_color.RelativeTime.Ticks);
                            if (latest_color.RelativeTime.Ticks > color_timestamp)
                            {
                                ROS_CS.sensor_msgs.Image color_image = GetColorImageFromRaw(latest_color);
                                if (color_image != null)
                                {
                                    Console.WriteLine("Sending color image");
                                    color_tx.Send(color_image);
                                }
                                else
                                {
                                    Console.WriteLine("Null color image");
                                }
                                color_timestamp = latest_color.RelativeTime.Ticks;
                            }
                        }
                    }
                }
                if (publish_skeleton)
                {
                    //Get skeleton data
                    BodyFrame latest_skeleton = skeleton_reader.AcquireLatestFrame();
                    if (latest_skeleton != null)
                    {
                        using (latest_skeleton)
                        {
                            Console.WriteLine("Current skeleton timestamp: {0:D}, new timestamp: {1:D}", skeleton_timestamp, latest_skeleton.RelativeTime.Ticks);
                            if (latest_skeleton.RelativeTime.Ticks > skeleton_timestamp)
                            {
                                ROS_CS.person_tracker.TrackerState skeleton_tracks = GetSkeletonTracksFromRaw(latest_skeleton);
                                if (skeleton_tracks != null)
                                {
                                    Console.WriteLine("Sending skeleton tracks");
                                    skeleton_tx.Send(skeleton_tracks);
                                }
                                else
                                {
                                    Console.WriteLine("Null skeleton tracks");
                                }
                                skeleton_timestamp = latest_skeleton.RelativeTime.Ticks;
                            }
                        }
                    }
                }
            }
        }

        static void ColorImageCB(ROS_CS.sensor_msgs.Image msg)
        {
            Console.WriteLine("Received image message");
            Console.WriteLine("Width: {0:D}\nHeight: {1:D}\nStep: {2:D}\nData length (real): {3:D}\nShould be: {4:D}", msg.width, msg.height, msg.step, msg.data.Count, (1920 * 1080 * 4));
            //Console.WriteLine(msg);
        }

        static ROS_CS.person_tracker.TrackerState GetSkeletonTracksFromRaw(BodyFrame new_body_frame)
        {
            ROS_CS.person_tracker.TrackerState skeleton_tracks = new ROS_CS.person_tracker.TrackerState();
            skeleton_tracks.header.frame_id = "kinect2_depth_optical_frame";
            skeleton_tracks.header.stamp = KinectTimestampsToROS(new_body_frame.RelativeTime.Ticks);
            skeleton_tracks.TrackerType = skeleton_tracks.SKELETON;
            skeleton_tracks.TrackerName = "Kinect2SDK";
            // Unpack + repack the BodyFrame into geometry_msgs/PoseArray form
            Body[] tracked_skeletons = new Body[new_body_frame.BodyFrameSource.BodyCount];
            new_body_frame.GetAndRefreshBodyData(tracked_skeletons);
            foreach (Body skeleton in tracked_skeletons)
            {
                if (skeleton.IsTracked)
                {
                    // Assemble a tracking message for the current skeleton track
                    // Make the person track message
                    ROS_CS.person_tracker.TrackedPerson current_skeleton = new ROS_CS.person_tracker.TrackedPerson();
                    current_skeleton.TrackerType = current_skeleton.SKELETON;
                    current_skeleton.TrackerName = "Kinect2SDK";
                    current_skeleton.header.frame_id = "kinect2_depth_optical_frame";
                    // Populate the hands
                    // Left hand
                    ROS_CS.person_tracker.HandTrack left_hand = new ROS_CS.person_tracker.HandTrack();
                    HandState left_hand_state = skeleton.HandLeftState;
                    switch (left_hand_state)
                    {
                        case HandState.Closed:
                            left_hand.HandState = left_hand.CLOSED;
                            break;
                        case HandState.Open:
                            left_hand.HandState = left_hand.OPEN;
                            break;
                        case HandState.Lasso:
                            left_hand.HandState = left_hand.CLOSED;
                            break;
                        case HandState.NotTracked:
                            left_hand.HandState = left_hand.UNKNOWN;
                            break;
                        case HandState.Unknown:
                            left_hand.HandState = left_hand.UNKNOWN;
                            break;
                    }
                    TrackingConfidence left_hand_state_confidence = skeleton.HandLeftConfidence;
                    switch (left_hand_state_confidence)
                    {
                        case TrackingConfidence.High:
                            left_hand.HandStateConfidence = 1.0f;
                            break;
                        case TrackingConfidence.Low:
                            left_hand.HandStateConfidence = 0.5f;
                            break;
                    }
                    // Right hand
                    ROS_CS.person_tracker.HandTrack right_hand = new ROS_CS.person_tracker.HandTrack();
                    HandState right_hand_state = skeleton.HandRightState;
                    switch (right_hand_state)
                    {
                        case HandState.Closed:
                            right_hand.HandState = right_hand.CLOSED;
                            break;
                        case HandState.Open:
                            right_hand.HandState = right_hand.OPEN;
                            break;
                        case HandState.Lasso:
                            right_hand.HandState = right_hand.CLOSED;
                            break;
                        case HandState.NotTracked:
                            right_hand.HandState = right_hand.UNKNOWN;
                            break;
                        case HandState.Unknown:
                            right_hand.HandState = right_hand.UNKNOWN;
                            break;
                    }
                    TrackingConfidence right_hand_state_confidence = skeleton.HandRightConfidence;
                    switch (right_hand_state_confidence)
                    {
                        case TrackingConfidence.High:
                            right_hand.HandStateConfidence = 1.0f;
                            break;
                        case TrackingConfidence.Low:
                            right_hand.HandStateConfidence = 0.5f;
                            break;
                    }
                    // Save the hands
                    current_skeleton.Hands.Add(left_hand);
                    current_skeleton.Hands.Add(right_hand);
                    // Make the skeleton track
                    ROS_CS.person_tracker.SkeletonTrack current_skeleton_track = new ROS_CS.person_tracker.SkeletonTrack();
                    // First, copy out the joint positions and orientations
                    IReadOnlyDictionary<JointType, Joint> joint_positions = skeleton.Joints;
                    IReadOnlyDictionary<JointType, JointOrientation> joint_orientations = skeleton.JointOrientations;
                    // Loop through the joints and add each one
                    if (joint_positions.Count != joint_orientations.Count)
                    {
                        Console.WriteLine("Number of joint positions and orientations is different - not populating track!");
                    }
                    else
                    {
                        foreach (JointType joint_type in joint_positions.Keys)
                        {
                            // Set the confidence value
                            TrackingState joint_tracking_state = joint_positions[joint_type].TrackingState;
                            switch (joint_tracking_state)
                            {
                                case TrackingState.Tracked:
                                    current_skeleton_track.Confidences.Add(1.0f);
                                    break;
                                case TrackingState.Inferred:
                                    current_skeleton_track.Confidences.Add(0.5f);
                                    break;
                                case TrackingState.NotTracked:
                                    current_skeleton_track.Confidences.Add(0.0f);
                                    break;
                            }
                            // Get the position and orientation of the joint
                            CameraSpacePoint joint_position = joint_positions[joint_type].Position;
                            Vector4 joint_orientation = joint_orientations[joint_type].Orientation;
                            ROS_CS.geometry_msgs.Transform joint_transform = new ROS_CS.geometry_msgs.Transform();
                            joint_transform.translation.x = joint_position.X;
                            joint_transform.translation.y = joint_position.Y;
                            joint_transform.translation.z = joint_position.Z;
                            joint_transform.rotation.x = joint_orientation.X;
                            joint_transform.rotation.y = joint_orientation.Y;
                            joint_transform.rotation.z = joint_orientation.Z;
                            joint_transform.rotation.w = joint_orientation.W;
                            // Set the name of the joint
                            current_skeleton_track.JointNames.Add(GetJointNameFromJointType(joint_type));
                            // Save it
                            current_skeleton_track.JointPositions.Add(joint_transform);
                        }
                    }
                    // Save the skeleton track
                    current_skeleton.Skeleton = current_skeleton_track;
                    // Set the name of the current track
                    current_skeleton.UID = (System.UInt32)skeleton.TrackingId;
                    // Save the person track
                    skeleton_tracks.Tracks.Add(current_skeleton);
                    // Print a status
                    Console.WriteLine("Actively tracking body #{0:D}", skeleton.TrackingId);
                }
                else
                {
                    Console.WriteLine("Not tracking body #{0:D}", skeleton.TrackingId);
                }
            }
            // Finish populating the track message
            skeleton_tracks.ActiveTracks = (System.UInt32)skeleton_tracks.Tracks.Count;
            return skeleton_tracks;
        }

        static string GetJointNameFromJointType(JointType new_joint_type)
        {
            switch (new_joint_type)
            {
                case JointType.AnkleLeft:
                    return "left_ankle";
                case JointType.AnkleRight:
                    return "right_ankle";
                case JointType.ElbowLeft:
                    return "left_elbow";
                case JointType.ElbowRight:
                    return "right_elbow";
                case JointType.FootLeft:
                    return "left_foot";
                case JointType.FootRight:
                    return "right_foot";
                case JointType.HandLeft:
                    return "left_hand";
                case JointType.HandRight:
                    return "right_hand";
                case JointType.HandTipLeft:
                    return "left_hand_tip";
                case JointType.HandTipRight:
                    return "right_hand_tip";
                case JointType.Head:
                    return "head";
                case JointType.HipLeft:
                    return "left_hip";
                case JointType.HipRight:
                    return "right_hip";
                case JointType.KneeLeft:
                    return "left_knee";
                case JointType.KneeRight:
                    return "right_knee";
                case JointType.Neck:
                    return "neck";
                case JointType.ShoulderLeft:
                    return "left_shoulder";
                case JointType.ShoulderRight:
                    return "right_shoulder";
                case JointType.SpineBase:
                    return "spine_base";
                case JointType.SpineMid:
                    return "spine_mid";
                case JointType.SpineShoulder:
                    return "spine_shoulders";
                case JointType.ThumbLeft:
                    return "left_thumb";
                case JointType.ThumbRight:
                    return "right_thumb";
                case JointType.WristLeft:
                    return "left_wrist";
                case JointType.WristRight:
                    return "right_wrist";
                default:
                    return "unknown";
            }
        }

        static ROS_CS.sensor_msgs.Image GetColorImageFromRaw(ColorFrame new_color_frame)
        {
            ROS_CS.sensor_msgs.Image color_image = new ROS_CS.sensor_msgs.Image();
            color_image.header.frame_id = "kinect2_color_optical_frame";
            color_image.header.stamp = KinectTimestampsToROS(new_color_frame.RelativeTime.Ticks);
            color_image.is_bigendian = 0;
            color_image.height = (uint)new_color_frame.FrameDescription.Height;
            color_image.width = (uint)new_color_frame.FrameDescription.Width;
            color_image.step = (uint)new_color_frame.FrameDescription.Width * 4;
            color_image.encoding = "rgba8";
            byte[] color_data = new byte[color_image.step * color_image.height];
            new_color_frame.CopyConvertedFrameDataToArray(color_data, ColorImageFormat.Rgba);
            color_image.data.AddRange(color_data);
            return color_image;
        }

        static ROS_CS.sensor_msgs.Image GetDepthImageFromRaw(DepthFrame new_depth_frame)
        {
            ROS_CS.sensor_msgs.Image depth_image = new ROS_CS.sensor_msgs.Image();
            depth_image.header.frame_id = "kinect2_depth_optical_frame";
            depth_image.header.stamp = KinectTimestampsToROS(new_depth_frame.RelativeTime.Ticks);
            depth_image.is_bigendian = 0;
            depth_image.height = (uint)new_depth_frame.FrameDescription.Height;
            depth_image.width = (uint)new_depth_frame.FrameDescription.Width;
            depth_image.step = (uint)new_depth_frame.FrameDescription.Width * 2;
            depth_image.encoding = "mono16";
            ushort[] depth_data = new ushort[new_depth_frame.FrameDescription.Height * new_depth_frame.FrameDescription.Width];
            new_depth_frame.CopyFrameDataToArray(depth_data);
            foreach (ushort depth in depth_data)
            {
                ushort cleaned_depth = (ushort)(depth >> 3);
                byte high_byte = (byte)((cleaned_depth & 0xFF00) >> 8);
                byte low_byte = (byte)(cleaned_depth & 0x00FF);
                depth_image.data.Add(high_byte);
                depth_image.data.Add(low_byte);
            }
            return depth_image;
        }

        static ROS_CS.Core.Time KinectTimestampsToROS(long kinect_timestamp)
        {
            ROS_CS.Core.Time timestamp = new ROS_CS.Core.Time();
            TimeSpan ts = new TimeSpan(kinect_timestamp);
            TimeSpan unix_ts = ts.Subtract(unix_base_ts);
            timestamp.secs = unix_ts.Seconds;
            timestamp.nsecs = unix_ts.Milliseconds * 1000000;
            return timestamp;
        }
    }
}
