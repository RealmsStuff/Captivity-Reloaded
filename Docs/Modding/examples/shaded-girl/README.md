# Shaded Girl draft example

This example demonstrates explicit Core asset replacement. It is not installable until the runtime asset resolver exists, and the referenced PNG files are intentionally not included.

The patch targets stable public player slots. It does not search for or overwrite matching Unity filenames. If another enabled pack targets the same slot, the loader must report a conflict and retain a known-good resolved asset until the user chooses a winner.
