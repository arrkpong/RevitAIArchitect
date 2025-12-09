# Revit AI Architect

Revit Add-in ที่เชื่อมต่อ AI เข้ากับ Autodesk Revit

## คุณสมบัติ

- **AI Chat Assistant:** ถามคำถามเกี่ยวกับ Revit หรือเรื่องทั่วไปได้โดยไม่ต้องออกจากโปรแกรม
- **WPF Interface:** หน้าต่างแชทที่ใช้งานง่าย
- **ขยายได้:** สามารถเปลี่ยน AI Provider ได้ (OpenAI, Azure, Local LLM)

## เริ่มต้นใช้งาน

### สิ่งที่ต้องมี

- Autodesk Revit 2026 (หรือเวอร์ชันที่รองรับ)
- Visual Studio 2022 (พร้อม .NET Desktop Development workload)
- OpenAI API Key

### การติดตั้ง

1. Clone repository นี้
2. เปิด `RevitAIArchitect.sln` ใน Visual Studio
3. Build solution
4. รันสคริปต์ `.\scripts\build_and_deploy.ps1`

## วิธีใช้งาน

1. เปิด Revit 2026
2. ไปที่แถบ **Add-Ins**
3. คลิก **Ask AI**
4. พิมพ์คำถามในหน้าต่างแชท

## License

โปรเจกต์นี้อยู่ภายใต้ MIT License - ดูรายละเอียดในไฟล์ LICENSE
