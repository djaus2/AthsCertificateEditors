# AthsCertificateEditors

A collection of editors and small tooling used to create and manage assets for the AthleticsCertificateMaker project.

Repository it supports
----------------------
This repository provides editors and helper utilities consumed by:
https://github.com/djaus2/AthleticsCertificateMaker

Purpose
-------
- Provide GUI and CLI editors, templates, and utilities for certificate artwork, text layout, and import/export workflows.
- Produce consistent assets and configuration files used by AthleticsCertificateMaker.

Key features
-----------
- Template editors for certificate layouts and text fields
- Exporters that generate images, JSON configuration, and font assets
- Utilities for validating and migrating certificate configuration files

Requirements
-----------
- .NET 10 SDK
- Visual Studio 2026 or a compatible IDE (solution present: AthsCertificateEditors.slnx)
- Windows (development tested on Windows with PowerShell)

Getting started
---------------
1. Clone the repository:
   git clone https://github.com/djaus2/AthsCertificateEditors.git

2. Open the solution in Visual Studio 2026:
   File → Open → Project/Solution → AthsCertificateEditors.slnx
   Or use the CLI:
   dotnet sln AthsCertificateEditors.slnx

3. Build:
   - Visual Studio: Build Solution
   - CLI: dotnet build

4. Run an editor:
   - Set the desired editor project as startup in Visual Studio and run.
   - Or run from the CLI: dotnet run --project <path-to-editor-project>

Usage
-----
- Use the editors to create or edit certificate templates and export resulting assets (images, JSON configuration, fonts) into the folder structure expected by AthleticsCertificateMaker.
- Consult the AthleticsCertificateMaker README for details about required output formats and folder conventions.

Contributing
------------
- Open an issue or submit a pull request.
- Follow the repository's coding style and target .NET 10.
- For changes that affect AthleticsCertificateMaker, reference that repository in the issue or PR.

Issues & Support
----------------
- Open issues here and reference the AthleticsCertificateMaker project when relevant:
  https://github.com/djaus2/AthleticsCertificateMaker

License
-------
See the LICENSE file in this repository for license details.