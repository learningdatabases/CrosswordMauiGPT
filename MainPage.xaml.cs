using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;

namespace CrosswordMauiGPT
{
         public partial class MainPage : ContentPage
        {
            private int _gridSize = 5; // Increase grid size to accommodate more complex placements
            private Dictionary<string, string> _termsAndDefinitions = new Dictionary<string, string>();
            private char[,] _solutionGrid;
            private Entry[,] _entryGrid;

            public MainPage()
            {
                InitializeComponent();
                InitializeTermsInput();
            }

            private void InitializeTermsInput()
            {
                for (int i = 0; i < 3; i++) // For simplicity, using 10 terms. Adjust as needed.
                {
                    var termEntry = new Entry { Placeholder = $"Term {i + 1}" };
                    var definitionEntry = new Entry { Placeholder = $"Definition {i + 1}" };

                    TermsStack.Children.Add(termEntry);
                    TermsStack.Children.Add(definitionEntry);
                }
            }

            private void OnGenerateCrosswordClicked(object sender, EventArgs e)
            {
                _termsAndDefinitions.Clear();
                for (int i = 0; i < TermsStack.Children.Count; i += 2)
                {
                    var term = ((Entry)TermsStack.Children[i]).Text;
                    var definition = ((Entry)TermsStack.Children[i + 1]).Text;

                    if (!string.IsNullOrEmpty(term) && !string.IsNullOrEmpty(definition))
                    {
                        _termsAndDefinitions[term] = definition;
                    }
                }

                InitializeGrid();
                PlaceWords();
                DisplayDefinitions();
            }

            private void InitializeGrid()
            {
                _solutionGrid = new char[_gridSize, _gridSize];
                _entryGrid = new Entry[_gridSize, _gridSize];
                CrosswordGrid.Children.Clear();
                CrosswordGrid.RowDefinitions.Clear();
                CrosswordGrid.ColumnDefinitions.Clear();

                for (int i = 0; i < _gridSize; i++)
                {
                    CrosswordGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    CrosswordGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }

                for (int row = 0; row < _gridSize; row++)
                {
                    for (int col = 0; col < _gridSize; col++)
                    {
                        var cell = new Entry
                        {
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center,
                            FontSize = 20
                        };
                        cell.TextChanged += OnCellTextChanged;
                        Grid.SetColumn(cell, col);
                        Grid.SetRow(cell, row);
                        CrosswordGrid.Children.Add(cell);
                        _entryGrid[row, col] = cell;
                        _solutionGrid[row, col] = '\0';
                    }
                }
            }

            private void PlaceWords()
            {
                var random = new Random();
                foreach (var term in _termsAndDefinitions.Keys)
                {
                    bool placed = false;
                    while (!placed)
                    {
                        int startRow = random.Next(_gridSize);
                        int startCol = random.Next(_gridSize);
                        int direction = random.Next(8); // 0: horizontal, 1: vertical, 2: diagonal, 3-7: reverse directions

                        if (direction == 0 && startCol + term.Length <= _gridSize) // Horizontal
                        {
                            placed = PlaceWord(term, startRow, startCol, 0, 1);
                        }
                        else if (direction == 1 && startRow + term.Length <= _gridSize) // Vertical
                        {
                            placed = PlaceWord(term, startRow, startCol, 1, 0);
                        }
                        else if (direction == 2 && startRow + term.Length <= _gridSize && startCol + term.Length <= _gridSize) // Diagonal
                        {
                            placed = PlaceWord(term, startRow, startCol, 1, 1);
                        }
                        else if (direction == 3 && startCol - term.Length >= -1) // Reverse Horizontal
                        {
                            placed = PlaceWord(term, startRow, startCol, 0, -1);
                        }
                        else if (direction == 4 && startRow - term.Length >= -1) // Reverse Vertical
                        {
                            placed = PlaceWord(term, startRow, startCol, -1, 0);
                        }
                        else if (direction == 5 && startRow - term.Length >= -1 && startCol - term.Length >= -1) // Reverse Diagonal
                        {
                            placed = PlaceWord(term, startRow, startCol, -1, -1);
                        }
                        else if (direction == 6 && startRow + term.Length <= _gridSize && startCol - term.Length >= -1) // Diagonal (down-left)
                        {
                            placed = PlaceWord(term, startRow, startCol, 1, -1);
                        }
                        else if (direction == 7 && startRow - term.Length >= -1 && startCol + term.Length <= _gridSize) // Diagonal (up-right)
                        {
                            placed = PlaceWord(term, startRow, startCol, -1, 1);
                        }
                    }
                }
            }

            private bool PlaceWord(string word, int startRow, int startCol, int rowDir, int colDir)
            {
                for (int i = 0; i < word.Length; i++)
                {
                    int newRow = startRow + i * rowDir;
                    int newCol = startCol + i * colDir;

                    if (_solutionGrid[newRow, newCol] != '\0' && _solutionGrid[newRow, newCol] != word[i])
                    {
                        return false;
                    }
                }

                for (int i = 0; i < word.Length; i++)
                {
                    int newRow = startRow + i * rowDir;
                    int newCol = startCol + i * colDir;
                    _solutionGrid[newRow, newCol] = word[i];
                }

                return true;
            }

            private void DisplayDefinitions()
            {
                DefinitionsStack.Children.Clear();
                foreach (var term in _termsAndDefinitions.Keys)
                {
                    var label = new Label
                    {
                        Text = $"{term}: {_termsAndDefinitions[term]}",
                        FontSize = 18,
                        Margin = new Thickness(5)
                    };
                    DefinitionsStack.Children.Add(label);
                }
            }

            private void OnCellTextChanged(object sender, TextChangedEventArgs e)
            {
                var cell = (Entry)sender;
                if (cell.Text.Length > 1)
                {
                    cell.Text = cell.Text.Substring(0, 1);
                }

                for (int row = 0; row < _gridSize; row++)
                {
                    for (int col = 0; col < _gridSize; col++)
                    {
                        if (_entryGrid[row, col] == cell)
                        {
                            if (cell.Text.Length == 1 && cell.Text[0] == _solutionGrid[row, col])
                            {
                                cell.BackgroundColor = Colors.Green;
                            }
                            else
                            {
                                cell.BackgroundColor = Colors.Red;
                            }
                            return;
                        }
                    }
                }
            }

            private void OnCheckSolutionsClicked(object sender, EventArgs e)
            {
                bool correct = true;
                for (int row = 0; row < _gridSize; row++)
                {
                    for (int col = 0; col < _gridSize; col++)
                    {
                        var cell = _entryGrid[row, col];
                        if (_solutionGrid[row, col] != '\0')
                        {
                            if (string.IsNullOrEmpty(cell.Text) || cell.Text[0] != _solutionGrid[row, col])
                            {
                                correct = false;
                            }
                        }
                    }
                }

                DisplayAlert("Check Solutions", correct ? "All solutions are correct!" : "Some solutions are incorrect.", "OK");
            }

            private void OnShowSolutionsClicked(object sender, EventArgs e)
            {
                for (int row = 0; row < _gridSize; row++)
                {
                    for (int col = 0; col < _gridSize; col++)
                    {
                        var cell = _entryGrid[row, col];
                        if (_solutionGrid[row, col] != '\0')
                        {
                            cell.Text = _solutionGrid[row, col].ToString();
                            cell.BackgroundColor = Colors.Green;
                        }
                    }
                }
            }
        }
    }

