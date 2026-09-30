# Dependencies and load order

Required dependencies load first. A missing, incompatible, invalid, conflicting, or disabled required dependency disables the dependent pack with an explanation.

Optional dependencies affect ordering only when a compatible enabled pack is present. Their absence does not disable the consumer.

`loadAfter` and `loadBefore` are soft ordering hints. Explicit `conflicts` and `overrides` should be used when two packs cannot safely coexist or intentionally target the same public slot.
