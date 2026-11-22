## ADDED Requirements

### Requirement: A key moves to the next session that needs input
`w` on the Sessions tab SHALL move the selection to the next shown session that needs input after the selected row, in the table's order, wrapping to the first; with none, the selection SHALL stay and the status line SHALL say that no session needs input. Headings SHALL be stepped over and the filter respected.

#### Scenario: A waiting session in another group
- **WHEN** fix-123 in Northwind.Orders is selected and only refund-retry in Northwind.Shipping, two groups below, needs input
- **THEN** `w` selects refund-retry, and Enter then opens it or switches to its tab

#### Scenario: Wrapping to the top
- **WHEN** the last row is selected and the first row's session needs input
- **THEN** `w` selects the first row

#### Scenario: Nothing waiting
- **WHEN** no shown session needs input
- **THEN** `w` leaves the selection where it is and the status line says no session needs input

#### Scenario: Under a filter
- **WHEN** the filter is orders and the only session needing input is outside it
- **THEN** `w` leaves the selection where it is and says no session needs input
