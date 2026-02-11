Feature: Board Management
  As a team member
  I want to view and interact with the Kanban board
  So that I can track task progress

  Scenario: View board columns
    Given I am logged in
    When I am on the board page
    Then I should see columns "Ideas", "Backlog", "In Progress", "Acceptance", "Done", "Rejected"

  Scenario: Add task button only on Ideas and Backlog
    Given I am logged in
    When I am on the board page
    Then I should see "Add Task" button on "Ideas" column
    And I should see "Add Task" button on "Backlog" column
    And I should not see "Add Task" button on "In Progress" column
    And I should not see "Add Task" button on "Done" column

  Scenario: Create a new task
    Given I am logged in
    When I click "Add Task" on the "Ideas" column
    And I fill in title "Test Task"
    And I set priority to "High"
    And I click Create
    Then I should see "Test Task" in the "Ideas" column

  Scenario: Edit a task
    Given I am logged in
    And there is a task "Existing Task" in "Ideas"
    When I click edit on "Existing Task"
    And I change the title to "Updated Task"
    And I click Save
    Then I should see "Updated Task" on the board

  Scenario: Delete a task shows glitter bomb
    Given I am logged in
    And there is a task "Delete Me" in "Ideas"
    When I click edit on "Delete Me"
    And I click Delete
    Then I should see the glitter bomb animation
    And "Delete Me" should not appear on the board
