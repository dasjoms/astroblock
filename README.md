# Astroblock

## Quick coordinate sanity demo

For quick manual validation of world/chunk/local coordinate conversions, run:

```bash
dotnet run --project src/Tools/Debug/CoordSanityDemo/CoordSanityDemo.csproj
```

The demo prints each world input, derived chunk/local values, reconstructed world output, and a PASS/FAIL marker.

## Module onboarding and coordinate boundary discipline

- Onboarding checklist: `docs/module-onboarding.md`
- Coordinate conversion authority rule: `docs/coords-authority.md`
- Static guard script: `scripts/check-coords-authority.sh`
