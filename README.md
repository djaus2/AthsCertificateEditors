# AthsCertificateEditors

A collection of editors and small tooling used to create and manage assets for the AthleticsCertificateMaker project.

Repository it supports

----------------------
This repository provides editors that produce json files, as Certificate Metadata, consumed by:
<https://github.com/djaus2/AthleticsCertificateMaker>

> Next step will be to move the certificate generation to WPF.

Purpose

-------

- [AthleticsCertificateMaker](https://github.com/djaus2/AthleticsCertificateMaker) now reads a json file to define the certificate metadata.
  - Previously the certificate layout and content was defined by a Powershell script in the project as above.
- This UI app will read in a existing config file as json, edit and save, Can also create a new one.
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

Included apps
-----------

This solution contains three small editor applications. Brief descriptions and where to run them follow.

- TxtEditor (TxtEditor\TxtEditor.csproj)
  - A simple raw text (JSON) editor for the certificate configuration. Features load, save, pretty-format and basic validation. Useful when you want to edit the JSON directly or start from a default document.
  - Run: set TxtEditor as the startup project in Visual Studio or dotnet run --project TxtEditor\TxtEditor.csproj

- FormEdit (FormEdit\FormEditCert.csproj)
  - A form-based editor that binds to a CertificateModel. Provides UI grids to edit CertificateText and CertificateFields, and common metadata (template file, CSV file, colors, flags). Produces the same JSON used by AthleticsCertificateMaker but with a user-friendly form interface.
  - Run: set FormEdit as the startup project in Visual Studio or dotnet run --project FormEdit\FormEditCert.csproj

- FoldingEditor (FoldingEditor\FoldingEditor.csproj)
  - A code-style JSON editor using AvalonEdit with syntax highlighting and folding. Good for large documents where folding and code-style editing help navigation. Supports load, save, pretty-format and JSON validation.
  - Run: set FoldingEditor as the startup project in Visual Studio or dotnet run --project FoldingEditor\FoldingEditor.csproj

Getting started

---------------

1. Clone the repository:
   git clone <https://github.com/djaus2/AthsCertificateEditors.git>

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
  <https://github.com/djaus2/AthleticsCertificateMaker>

License

-------
See the LICENSE file in this repository for license details.
