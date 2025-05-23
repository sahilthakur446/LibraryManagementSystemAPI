﻿using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.DTOs.Book
{
    public class BookCopyResponseDTO
    {
        public int CopyId { get; set; }

        public int BookId { get; set; }

        public bool IsAvailable { get; set; }
        public string BookName { get; set; }
        public BookDTO? Book { get; set; } = null;
    }
}
