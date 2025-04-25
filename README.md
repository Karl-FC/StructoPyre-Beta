# StructoPyre

**An Integrated Fire Simulation Web-App for Structural Integrity and Evacuation Dynamics (with reference to ACI 216.1-14(19))**

---

## 📖 How to Use
See the [Howto.md](./Howto.md) for detailed usage instructions.

---

## Overview

StructoPyre is a next-generation, accessible fire simulation web application designed to analyze the structural integrity of concrete buildings and evacuation dynamics during fire scenarios. The app leverages the ACI 216.1-14(19) standard to provide real-time, visual feedback on fire resistance and safety, making advanced fire engineering tools available to a broader audience.

---

## Table of Contents

- [Background](#background)
- [Features](#features)
- [Delimitations](#delimitations)
- [How It Works](#how-it-works)
- [Installation & Usage](#installation--usage)
- [Technical Stack](#technical-stack)
- [Research & Methodology](#research--methodology)
- [References](#references)
- [License](#license)

---

## Background

Traditional fire simulation and structural analysis software are often expensive, complex, and inaccessible to many users. StructoPyre addresses these issues by providing a web-based, user-friendly, and standards-compliant simulation tool, focusing on:

- **Structural fire resistance** (per ACI 216.1-14(19))
- **Evacuation dynamics** in fire scenarios
- **Accessibility** for both professionals and non-experts

---

## Features

- **Import 3D Models:** Supports `.obj` (and `.mtl`) file import for building geometry.
- **Automatic & Manual Scaling:** Intelligent unit detection and manual override for model scaling.
- **Material Mapping:** Map imported materials to real-world ACI aggregate types and assign structural properties.
- **Simulation Engine:** Visualizes fire progression and checks structural elements against ACI fire-resistance requirements in real time.
- **Evacuation Dynamics:** Simulate and visualize occupant movement and egress during fire events.
- **First-Person Exploration:** Navigate imported models in first-person view.
- **Visual Feedback:** Elements change color (e.g., green = safe, red = failed) based on compliance with ACI requirements as the simulation progresses.
- **Cross-Platform:** Runs in modern browsers (WebGL) and as a standalone app.

---

## Delimitations

- Focused on **concrete structures** (no steel, timber, or composite).
- Adheres to **ACI 216.1-14(19)** provisions only.
- Does **not** perform advanced structural analysis beyond fire resistance and evacuation.
- Simulation parameters are restricted to those defined in the ACI standard.
- Real-world variables (e.g., wind, smoke, human behavior) are not modeled in detail.

---

## How It Works

1. **Import Model:** Upload your building’s `.obj` file (optionally with `.mtl`).
2. **Assign Properties:** Use the UI to map model parts to ACI element types (slab, beam, wall, column), aggregate types, and input required properties (cover, thickness, restraint, etc.).
3. **Start Simulation:** Place fire sources and run the simulation.
4. **Real-Time Analysis:** The app checks each element’s fire resistance against ACI tables/formulas for the current simulation time.
5. **Visualize Results:** Elements that fail ACI requirements are highlighted, and evacuation routes are dynamically updated.

---

## Installation & Usage

### Web Version

1. Visit the hosted web app (URL TBD).
2. Import your `.obj` model.
3. Follow the on-screen instructions to assign properties and run simulations.

### Local/Standalone

1. Go to Releases and download the latest version
2. Extract the `.zip` file
3. Open `index.html`

---

## Technical Stack

- **Unity 6** (C#) — Core simulation and 3D engine
- **React (JavaScript)** — Web frontend (for hosting site)
- **Google Firebase / Vercel** — Backend hosting (for web version)
- **SFB (StandaloneFileBrowser)** — For file import in WebGL/Standalone
- **Dummiesman OBJ Loader** — For 3D model import

---

## Research & Methodology

- **Standards-Based:** Implements ACI 216.1-14(19) for fire resistance of concrete and masonry assemblies.
- **User-Centered Design:** Focus on accessibility, ease of use, and minimal training required.
- **Validation:** Pilot and field testing with fire safety professionals; feedback-driven development.
- **Social Acceptability:** Survey-based assessment of usability and impact.

---

## References

- **ACI 216.1-14(19):** Standard Method for Determining Fire Resistance of Concrete and Masonry Construction Assemblies.
- [See full research paper for detailed references and methodology.](#)

---

## License

MIT License. See [LICENSE](LICENSE) for details.

---

## Acknowledgments

Developed by Bakal-Area.net, Karl-FC and collaborators. Special thanks to fire safety professionals, all field testers,  our thesis advisor, and DA LAB.

---

**For more information, see the full research paper or contact the development team.**

---

Let me know if you want a more technical section (e.g., code structure, API usage) or a quickstart guide for developers!
