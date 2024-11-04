import threading
import socket
import cv2 
import numpy as np        
import mediapipe as mp
from dollarpy import Recognizer, Template, Point
import pickle
import struct

global recognized_text
recognized_text = ""
conn = None
mp_drawing = mp.solutions.drawing_utils
mp_holistic = mp.solutions.holistic

gesture_templates = {
    "OneFinger": ["./vids/OneFinger1.mp4", "./vids/OneFinger2.mp4", "./vids/OneFinger3.mp4"],
    "TwoFingers": ["./vids/TwoFingers1.mp4", "./vids/TwoFingers2.mp4", "./vids/TwoFingers3.mp4"],
    "ThreeFingers": ["./vids/ThreeFingers1.mp4", "./vids/ThreeFingers2.mp4", "./vids/ThreeFingers3.mp4", "./vids/ThreeFingers4.mp4", "./vids/ThreeFingers5.mp4"]
}
def capture_gesture_points(video_files, gesture_type):
    points = []
    for video_path in video_files:
        cap = cv2.VideoCapture(video_path)
        with mp_holistic.Holistic(min_detection_confidence=0.5, min_tracking_confidence=0.5) as holistic:
            while cap.isOpened():
                ret, frame = cap.read()
                if not ret:
                    break

                image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
                results = holistic.process(image)

                if results.right_hand_landmarks or results.left_hand_landmarks:
                    hand_landmarks = results.right_hand_landmarks or results.left_hand_landmarks
                    index_tip = hand_landmarks.landmark[mp.solutions.hands.HandLandmark.INDEX_FINGER_TIP]
                    middle_tip = hand_landmarks.landmark[mp.solutions.hands.HandLandmark.MIDDLE_FINGER_TIP]
                    ring_tip = hand_landmarks.landmark[mp.solutions.hands.HandLandmark.RING_FINGER_TIP]
                    pinky_tip = hand_landmarks.landmark[mp.solutions.hands.HandLandmark.PINKY_TIP]

                    if gesture_type == "OneFinger":
                        if (index_tip.y < hand_landmarks.landmark[mp.solutions.hands.HandLandmark.INDEX_FINGER_MCP].y and
                            middle_tip.y > hand_landmarks.landmark[mp.solutions.hands.HandLandmark.MIDDLE_FINGER_MCP].y and
                            ring_tip.y > hand_landmarks.landmark[mp.solutions.hands.HandLandmark.RING_FINGER_MCP].y and
                            pinky_tip.y > hand_landmarks.landmark[mp.solutions.hands.HandLandmark.PINKY_MCP].y):
                            points.append(Point(index_tip.x, index_tip.y, 1))

                    elif gesture_type == "TwoFingers":
                        if (index_tip.y < hand_landmarks.landmark[mp.solutions.hands.HandLandmark.INDEX_FINGER_MCP].y and
                            middle_tip.y < hand_landmarks.landmark[mp.solutions.hands.HandLandmark.MIDDLE_FINGER_MCP].y and
                            ring_tip.y > hand_landmarks.landmark[mp.solutions.hands.HandLandmark.RING_FINGER_MCP].y and
                            pinky_tip.y > hand_landmarks.landmark[mp.solutions.hands.HandLandmark.PINKY_MCP].y):
                            points.extend([Point(index_tip.x, index_tip.y, 1), Point(middle_tip.x, middle_tip.y, 1)])

                    elif gesture_type == "ThreeFingers":
                        if (index_tip.y < hand_landmarks.landmark[mp.solutions.hands.HandLandmark.INDEX_FINGER_MCP].y and
                            middle_tip.y < hand_landmarks.landmark[mp.solutions.hands.HandLandmark.MIDDLE_FINGER_MCP].y and
                            ring_tip.y < hand_landmarks.landmark[mp.solutions.hands.HandLandmark.RING_FINGER_MCP].y and
                            pinky_tip.y > hand_landmarks.landmark[mp.solutions.hands.HandLandmark.PINKY_MCP].y):
                            points.extend([Point(index_tip.x, index_tip.y, 1), Point(middle_tip.x, middle_tip.y, 1), Point(ring_tip.x, ring_tip.y, 1)])

                cv2.imshow("Training Gesture", cv2.cvtColor(image, cv2.COLOR_RGB2BGR))
                if cv2.waitKey(10) & 0xFF == ord('q'):
                    break

        cap.release()
        cv2.destroyAllWindows()

    return points
templates = []
for gesture_type, video_paths in gesture_templates.items():
    points = capture_gesture_points(video_paths, gesture_type)
    if points:
        templates.append(Template(gesture_type, points))

model_filename = 'gesture_templates.pkl'
with open(model_filename, 'wb') as model_file:
    pickle.dump(templates, model_file)
print(f"Model saved to {model_filename}")


def detect_gestures_real_time(soc):
    cap = cv2.VideoCapture(0)
    recognizer = Recognizer(templates)

    with mp_holistic.Holistic(min_detection_confidence=0.5, min_tracking_confidence=0.5) as holistic:
        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                break

            frame = cv2.flip(frame, 1)
            image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            results = holistic.process(image)
            image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)

            detected_gesture = "No gesture"
            if results.right_hand_landmarks or results.left_hand_landmarks:
                hand_landmarks = results.right_hand_landmarks or results.left_hand_landmarks
                points = []

                # Get fingertip landmarks
                index_tip = hand_landmarks.landmark[mp.solutions.hands.HandLandmark.INDEX_FINGER_TIP]
                middle_tip = hand_landmarks.landmark[mp.solutions.hands.HandLandmark.MIDDLE_FINGER_TIP]
                ring_tip = hand_landmarks.landmark[mp.solutions.hands.HandLandmark.RING_FINGER_TIP]
                pinky_tip = hand_landmarks.landmark[mp.solutions.hands.HandLandmark.PINKY_TIP]

                # Draw circles on specific fingertip points
                def draw_circle(landmark, color):
                    cx, cy = int(landmark.x * frame.shape[1]), int(landmark.y * frame.shape[0])
                    cv2.circle(image, (cx, cy), 10, color, -1)

                # Detect gestures and show points for each gesture
                if (index_tip.y < hand_landmarks.landmark[mp.solutions.hands.HandLandmark.INDEX_FINGER_MCP].y and
                    middle_tip.y > hand_landmarks.landmark[mp.solutions.hands.HandLandmark.MIDDLE_FINGER_MCP].y and
                    ring_tip.y > hand_landmarks.landmark[mp.solutions.hands.HandLandmark.RING_FINGER_MCP].y and
                    pinky_tip.y > hand_landmarks.landmark[mp.solutions.hands.HandLandmark.PINKY_MCP].y):
                    points.append(Point(index_tip.x, index_tip.y, 1))
                    detected_gesture = "One Finger Up"
                    draw_circle(index_tip, (0, 255, 0))  # Green circle for One Finger Up

                elif (index_tip.y < hand_landmarks.landmark[mp.solutions.hands.HandLandmark.INDEX_FINGER_MCP].y and
                      middle_tip.y < hand_landmarks.landmark[mp.solutions.hands.HandLandmark.MIDDLE_FINGER_MCP].y and
                      ring_tip.y > hand_landmarks.landmark[mp.solutions.hands.HandLandmark.RING_FINGER_MCP].y and
                      pinky_tip.y > hand_landmarks.landmark[mp.solutions.hands.HandLandmark.PINKY_MCP].y):
                    points.extend([Point(index_tip.x, index_tip.y, 1), Point(middle_tip.x, middle_tip.y, 1)])
                    detected_gesture = "Two Fingers Up"
                    draw_circle(index_tip, (0, 0, 255))     # Red circle for index finger
                    draw_circle(middle_tip, (255, 0, 0))   # Blue circle for middle finger

                elif (index_tip.y < hand_landmarks.landmark[mp.solutions.hands.HandLandmark.INDEX_FINGER_MCP].y and
                      middle_tip.y < hand_landmarks.landmark[mp.solutions.hands.HandLandmark.MIDDLE_FINGER_MCP].y and
                      ring_tip.y < hand_landmarks.landmark[mp.solutions.hands.HandLandmark.RING_FINGER_MCP].y and
                      pinky_tip.y > hand_landmarks.landmark[mp.solutions.hands.HandLandmark.PINKY_MCP].y):
                    points.extend([Point(index_tip.x, index_tip.y, 1), Point(middle_tip.x, middle_tip.y, 1), Point(ring_tip.x, ring_tip.y, 1)])
                    detected_gesture = "Three Fingers Up"

                    draw_circle(index_tip, (255, 255, 0))  # Cyan for index
                    draw_circle(middle_tip, (0, 255, 255)) # Yellow for middle
                    draw_circle(ring_tip, (255, 0, 255))   # Magenta for ring

            cv2.putText(image, f"Gesture: {detected_gesture}", (10, 40), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
            message = f"Gesture: {detected_gesture}"
            soc.sendall(message.encode())

            mp_drawing.draw_landmarks(image, results.right_hand_landmarks, mp_holistic.HAND_CONNECTIONS)
            mp_drawing.draw_landmarks(image, results.left_hand_landmarks, mp_holistic.HAND_CONNECTIONS)

           
            cv2.imshow("Gesture Detection", image)

            if cv2.waitKey(10) & 0xFF == ord('q'):
                break

    cap.release()
    cv2.destroyAllWindows()
def is_one_finger_up(hand_landmarks):
    tips_ids = [4, 8, 12, 16, 20]
    mcp_ids = [2, 5, 9, 13, 17]
    fingers_up = 0
    for tip, mcp in zip(tips_ids, mcp_ids):
        if hand_landmarks.landmark[tip].y < hand_landmarks.landmark[mcp].y:
            if tip == 8:
                fingers_up += 1
            else:
                return False
    return fingers_up == 1


def capture_hand_coordinates_and_detect_gestures_real_time(soc):
    global recognized_text
    mp_drawing = mp.solutions.drawing_utils
    mp_hands = mp.solutions.hands
    hands = mp_hands.Hands(min_detection_confidence=0.8, min_tracking_confidence=0.5)
    cap = cv2.VideoCapture(0, cv2.CAP_DSHOW)
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)

    while True:
        ret, frame = cap.read()
        if not ret:
            continue
        frame = cv2.flip(frame, 1)

        image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        results = hands.process(image)

        if recognized_text == "Abdelrahman":
            cv2.putText(frame, recognized_text, (20, 450), cv2.FONT_HERSHEY_SIMPLEX, 2, (0, 0, 255), 3)

        hand_coordinates = ""
        if results.multi_hand_landmarks:
            for hand_landmarks in results.multi_hand_landmarks:
                mp_drawing.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)
                coordinates = ",".join([f"{landmark.x},{landmark.y},{landmark.z}" for landmark in hand_landmarks.landmark[8:9]])
                hand_coordinates += coordinates + ";"
                if is_one_finger_up(hand_landmarks):
                    captured_image_path = "captured_image.jpg"
                    cv2.imwrite(captured_image_path, frame)
                    if recognized_text:
                        try:
                            message = f"{recognized_text}"
                            soc.sendall(message.encode())
                        except (BrokenPipeError, ConnectionResetError):
                            print("Connection lost. Exiting...")
                            break
        
        try:
            if hand_coordinates:
                data = hand_coordinates.encode('utf-8')
                size = struct.pack('>I', len(data))
                soc.sendall(size + data)
                detect_gestures_real_time(conn)

        except Exception as e:
            print(f"Error sending data: {e}")

        cv2.imshow('Hand Tracking and ', frame)
        key = cv2.waitKey(1)
        if key == ord('q'):
            break

    cap.release()
    cv2.destroyAllWindows()

def socket_listener():
    global conn
    mySocket = socket.socket()
    mySocket.bind(("localhost", 5050))
    mySocket.listen(5)
    print("Waiting for connection...")
    conn, addr = mySocket.accept()
    print("Device connected")
    capture_hand_coordinates_and_detect_gestures_real_time(conn)
    detect_gestures_real_time(conn)
    conn.close()
    mySocket.close()

thread = threading.Thread(target=socket_listener)
thread.start()
thread.join()
