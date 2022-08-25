#include <Servo.h>

Servo servo;

bool isRotating = false;
float steeringAngle = 0;
float angle = 0;

int servoPin = 9;

int prevTime = 0;

void setup() {

Serial.begin(9600);

servo.attach(servoPin);

}

void loop() {

  if(Serial.available())
  {
    String data = Serial.readString();

    if (data == "StopRotation"){
      isRotating = false;
      steeringAngle = 0;
      angle = 0;
    }
    
    else{
      isRotating = true;
      float value = data.toFloat();
      steeringAngle = value;
      angle = 0;
  }


  if (isRotating){
    angle += steeringAngle;
    servo.write(angle);
  }
  
  }
}
