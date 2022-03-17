

create database interview

create table tbl_interview
(
ApplicantId int Primary key identity,
ApplicantName varchar(50),
Gender varchar(20),
Phone varchar(20),
Email varchar(20),
InterviewDate varchar(20),
InterviewStatus varchar(20)
)

select * from tbl_interview