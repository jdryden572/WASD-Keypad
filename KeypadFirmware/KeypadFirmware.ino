// Message codes for simple serial comms
#define KEY_DOWN_START 243
#define KEY_UP_START 249
#define HANDSHAKE_REQ 224
#define HANDSHAKE_ACK 225

#include <Bounce2.h>

const int buttonPins[6] = {23, 22, 0, 1, 2, 3};
const int ledPins[6] = {20, 17, 16, 10, 9, 6};
const int ledIntensities[6] = {30, 30, 30, 30, 30, 30};
const int debounceInterval = 10;
Bounce buttons[6] = {Bounce(), Bounce(), Bounce(), Bounce(), Bounce(), Bounce()};

int ledStates[6] = {0, 0, 0, 0, 0, 0};
byte receivedByte = 0;
bool newData = false;

void setup() {
  // Initialize all buttons and LEDs
  for(int i=0; i<6; i++) {
    pinMode(buttonPins[i], INPUT_PULLUP);
    buttons[i].attach(buttonPins[i]);
    buttons[i].interval(debounceInterval);
    pinMode(ledPins[i], OUTPUT);
  }
  Serial.begin(115200);
}

void loop() {
  for(int i=0; i<6; i++) {
    buttons[i].update();
    updateState(i);
  }
  recvOneByte();
  handleCommand();
}

void updateState(int buttonNum) {
  // Called when buttons are first pressed
  if (buttons[buttonNum].fell()) {
    Serial.write(KEY_DOWN_START + buttonNum);
    switch (buttonNum) {
      case 0:
        sendLockKeystroke();
        analogWrite(ledPins[buttonNum], ledIntensities[buttonNum]);
        break;
      case 3:
        break;
      case 5:
        //sendMuteKeystroke();
        break;
      default:
        analogWrite(ledPins[buttonNum], ledIntensities[buttonNum]);
        break;
    }
  }

  // Called when button is released
  if (buttons[buttonNum].rose()) {
    Serial.write(KEY_UP_START + buttonNum);
    switch (buttonNum) {
      case 3:
      case 5:
        break;
      default:
        analogWrite(ledPins[buttonNum], 0);
        break;
    }
  }
}

void recvOneByte() {
  if (Serial.available() > 0) {
    receivedByte = Serial.read();
    newData = true;
  }
}

void handleCommand() {
  if (newData == true) {
    if(receivedByte == HANDSHAKE_REQ) {
      // Respond to handshake
      Serial.write(HANDSHAKE_ACK);
    }
    else {
      // Apply LED state
      for(int i=0; i<6; i++) {
        bool ledOn = receivedByte & (1 << i);
        analogWrite(ledPins[i], ledOn * ledIntensities[i]);
      }
    }  
    newData = false;
  }
}

void sendLockKeystroke() {
  Keyboard.set_modifier(MODIFIERKEY_GUI);
  Keyboard.send_now();
  Keyboard.set_key1(KEY_L);
  Keyboard.send_now();
  Keyboard.set_modifier(0);
  Keyboard.set_key1(0);
  Keyboard.send_now();
}

void sendMuteKeystroke() {
  Keyboard.set_modifier(MODIFIERKEY_GUI);
  Keyboard.send_now();
  Keyboard.set_key1(KEY_F4);
  Keyboard.send_now();
  Keyboard.set_modifier(0);
  Keyboard.set_key1(0);
  Keyboard.send_now();
}

