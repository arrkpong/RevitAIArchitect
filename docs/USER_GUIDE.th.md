# คู่มือการใช้งาน

คู่มือนี้อธิบายวิธีการติดตั้งและใช้งาน Revit AI Architect Add-in

## การติดตั้ง

### สิ่งที่ต้องมี

- Autodesk Revit 2026 (หรือเวอร์ชันที่รองรับ)
- Windows 10/11

### วิธีที่ 1: ใช้สคริปต์ (แนะนำ)

1. เปิด PowerShell ในโฟลเดอร์โปรเจกต์
2. รันสคริปต์ Deploy:
   ```powershell
   .\scripts\build_and_deploy.ps1
   ```
3. สคริปต์จะ Build และ Deploy ให้อัตโนมัติ

### วิธีที่ 2: ติดตั้งเอง

1. Build โปรเจกต์ใน Visual Studio (Debug หรือ Release)
2. Copy ไฟล์ `RevitAIArchitect.dll` จาก `bin\Debug\net8.0-windows\` ไปที่:
   ```
   %APPDATA%\Autodesk\Revit\Addins\2026\
   ```
3. สร้างไฟล์ `.addin` หรือ Copy ไฟล์ที่สร้างจากสคริปต์

## วิธีใช้งาน

### เปิดหน้าต่างแชท AI

1. เปิด Revit 2026
2. ไปที่แถบ **Add-Ins** บน Ribbon
3. คลิก **External Tools** → **Ask AI**
4. หน้าต่างแชทจะปรากฏขึ้น

### การแชทกับ AI

- พิมพ์คำถามในช่องด้านล่าง
- กดปุ่ม **Send** หรือ Enter
- รอคำตอบจาก AI

### ตัวอย่างคำถาม

- "วิธีสร้างกำแพงใน Revit ที่ดีที่สุดคืออะไร?"
- "ฉันจะ Export Schedule ไป Excel ได้อย่างไร?"
- "อธิบายความแตกต่างระหว่าง Hosted และ Non-hosted Family"

## การตั้งค่า

### ตั้งค่า API Key

1. เปิดไฟล์ `AiService.cs` ในโปรเจกต์
2. หาบรรทัด:
   ```csharp
   private const string ApiKey = "YOUR_OPENAI_API_KEY_HERE";
   ```
3. แทนที่ด้วย API Key ของ OpenAI ของคุณ
4. Build โปรเจกต์ใหม่

### เปลี่ยนโมเดล AI

ค่าเริ่มต้นใช้ `gpt-4o` ถ้าต้องการเปลี่ยน:

1. เปิดไฟล์ `AiService.cs`
2. หา `model = "gpt-4o"`
3. เปลี่ยนเป็นโมเดลที่ต้องการ (เช่น `gpt-3.5-turbo`)

## การแก้ปัญหา

### Add-in ไม่ปรากฏใน Revit

- ตรวจสอบว่าไฟล์ `.addin` อยู่ในโฟลเดอร์ที่ถูกต้อง
- ตรวจสอบ Path ของ DLL ในไฟล์ `.addin`
- รีสตาร์ท Revit

### Error "API Key not found"

- ตรวจสอบว่าตั้งค่า API Key ถูกต้องใน `AiService.cs`
- Build และ Deploy โปรเจกต์ใหม่

### หน้าต่างแชท Crash

- ตรวจสอบ Journal File ของ Revit
- ตรวจสอบการเชื่อมต่ออินเทอร์เน็ต
