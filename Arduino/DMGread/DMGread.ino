// Data Pins
int const data0 = 39;
int const data1 = 41;
int const data2 = 43;
int const data3 = 45;
int const data4 = 47;
int const data5 = 49;
int const data6 = 51;
int const data7 = 53;

// Address Pins
int const address0 = 22;
int const address1 = 24;
int const address2 = 26;
int const address3 = 28;
int const address4 = 30;
int const address5 = 32;
int const address6 = 34;
int const address7 = 36;
int const address8 = 38;
int const address9 = 40;
int const address10 = 42;
int const address11 = 44;
int const address12 = 46;
int const address13 = 48;
int const address14 = 50;
int const address15 = 52;

// Control pins
int const WR = 10;
int const RD = 11;
int const CL = 7;

// Onboard LED status pin
int const LED = 13;

void setup() {
  // put your setup code here, to run once:
  // Data Pins 0-7: 39 41 43 45 47 49 51 53
  // Adr Pins 0-15: 22 24 26 28 30 32 34 36 38 40 42 44 46 48 50 52
  // RD/WR = 11/10

  pinMode(WR, OUTPUT);
  pinMode(RD, OUTPUT);
  pinMode(CL, OUTPUT);

  pinMode(LED, OUTPUT);

  SetDataInputs();

  pinMode(address0, OUTPUT);
  pinMode(address1, OUTPUT);
  pinMode(address2, OUTPUT);
  pinMode(address3, OUTPUT);
  pinMode(address4, OUTPUT);
  pinMode(address5, OUTPUT);
  pinMode(address6, OUTPUT);
  pinMode(address7, OUTPUT);
  pinMode(address8, OUTPUT);
  pinMode(address9, OUTPUT);
  pinMode(address10, OUTPUT);
  pinMode(address11, OUTPUT);
  pinMode(address12, OUTPUT);
  pinMode(address13, OUTPUT);
  pinMode(address14, OUTPUT);
  pinMode(address15, OUTPUT);

  // Fastest baud that works reliably.
  Serial.begin(57600);

  // Default to reading ROM
  digitalWrite(RD, LOW);
  digitalWrite(WR, HIGH);
  digitalWrite(CL, LOW);
}

void SetDataInputs()
{
  pinMode(data0, INPUT);
  pinMode(data1, INPUT);
  pinMode(data2, INPUT);
  pinMode(data3, INPUT);
  pinMode(data4, INPUT);
  pinMode(data5, INPUT);
  pinMode(data6, INPUT);
  pinMode(data7, INPUT);
}

void SetDataOutputs()
{
  pinMode(data0, OUTPUT);
  pinMode(data1, OUTPUT);
  pinMode(data2, OUTPUT);
  pinMode(data3, OUTPUT);
  pinMode(data4, OUTPUT);
  pinMode(data5, OUTPUT);
  pinMode(data6, OUTPUT);
  pinMode(data7, OUTPUT);
}

void LowerDataOutputs()
{
  digitalWrite(data0, LOW);
  digitalWrite(data1, LOW);
  digitalWrite(data2, LOW);
  digitalWrite(data3, LOW);
  digitalWrite(data4, LOW);
  digitalWrite(data5, LOW);
  digitalWrite(data6, LOW);
  digitalWrite(data7, LOW);
}

void serialEvent()
{
  if (Serial.available() == 4)
  {
    word address = Serial.read() | Serial.read() << 8;
    byte data = Serial.read();
    byte control = Serial.read();

    // Control byte is eight flags:
    // 76543210
    // 16= Write data byte to address in address bytes
    // 8 = Select ram bank number, number is the data packet
    // 4 = Select rom bank number, number is the data packet
    // 2 = Dump the byte requested by address.
    // 1 = Dump the number requested number of bytes from the address, number of bytes in the data packet.

    if (control & 1)
    {
      ReadBytes(address, data);
    }

    if (control & 2)
    {
      Serial.write(ReadByte(address));
    }

    if (control & 4)
    {
      SwitchBank(data, false);
    }

    if (control & 8)
    {
      SwitchBank(data, true);
    }

    if (control & 16)
    {
      //WriteByte(address, data);
    }

    Serial.flush();
  }

}

void SetAddress(word address)
{
  digitalWrite(address0, bitRead(address, 0));
  digitalWrite(address1, bitRead(address, 1));
  digitalWrite(address2, bitRead(address, 2));
  digitalWrite(address3, bitRead(address, 3));
  digitalWrite(address4, bitRead(address, 4));
  digitalWrite(address5, bitRead(address, 5));
  digitalWrite(address6, bitRead(address, 6));
  digitalWrite(address7, bitRead(address, 7));
  digitalWrite(address8, bitRead(address, 8));
  digitalWrite(address9, bitRead(address, 9));
  digitalWrite(address10, bitRead(address, 10));
  digitalWrite(address11, bitRead(address, 11));
  digitalWrite(address12, bitRead(address, 12));
  digitalWrite(address13, bitRead(address, 13));
  digitalWrite(address14, bitRead(address, 14));
  digitalWrite(address15, bitRead(address, 15));
}

void WriteData(byte data)
{
  if (data & 1) {
    digitalWrite(data0, HIGH);
  }
  if (data & 2) {
    digitalWrite(data1, HIGH);
  }
  if (data & 4) {
    digitalWrite(data2, HIGH);
  }
  if (data & 8) {
    digitalWrite(data3, HIGH);
  }
  if (data & 16) {
    digitalWrite(data4, HIGH);
  }
  if (data & 32) {
    digitalWrite(data5, HIGH);
  }
  if (data & 64) {
    digitalWrite(data6, HIGH);
  }
  if (data & 128) {
    digitalWrite(data7, HIGH);
  }
}

// This works for MBC and MBC3
void SwitchBank(int bank, bool ram)
{
  digitalWrite(LED, HIGH);
  
  word address = 0x2000;

  if(ram)
  {
    address = 0x4000;
  }

  digitalWrite(RD, HIGH);
  digitalWrite(WR, LOW);

  delayMicroseconds(20);

  SetDataOutputs();
  
  SetAddress(address);

  delay(5);

  // Select the bank
  WriteData(bank);
  
  delayMicroseconds(20);

  digitalWrite(RD, LOW);
  digitalWrite(WR, HIGH);

  LowerDataOutputs();

  SetDataInputs();

  delayMicroseconds(20);

  digitalWrite(LED, LOW);
}

void ReadBytes(word address, int count)
{
  byte bundle[count];
  for (int i = 0; i < count; i++)
  {
    bundle[i] = ReadByte(address + i);
  }
  Serial.write(bundle, count);
}

byte ReadByte(word address)
{
  digitalWrite(LED, HIGH);
  
  word returnData = 0;

  SetAddress(address);

  delayMicroseconds(20);

  bitWrite(returnData, 0, digitalRead(data0));
  bitWrite(returnData, 1, digitalRead(data1));
  bitWrite(returnData, 2, digitalRead(data2));
  bitWrite(returnData, 3, digitalRead(data3));
  bitWrite(returnData, 4, digitalRead(data4));
  bitWrite(returnData, 5, digitalRead(data5));
  bitWrite(returnData, 6, digitalRead(data6));
  bitWrite(returnData, 7, digitalRead(data7));

  digitalWrite(LED, LOW);

  return returnData;
}

void loop() {

}
