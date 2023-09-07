import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { SelectList } from 'src/app/pages/common/common';
import { PMService } from '../../pm.services';
import { IPlanReportDetailDto } from '../plan-report-today/IplanReportDetai';
import { OrganizationService } from 'src/app/pages/common/organization/organization.service';
import { IPlannedReport } from '../planned-report/planned-report';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-progress-report-bystructure',
  templateUrl: './progress-report-bystructure.component.html',
  styleUrls: ['./progress-report-bystructure.component.css']
})
export class ProgressReportBystructureComponent implements OnInit {

  serachForm!: FormGroup
  progressReportByStructure  !: any
  branchs!: SelectList[]
  structures !: SelectList[]
  cnt: number = 0
  programs !: SelectList[]


  constructor(
    private formBuilder: FormBuilder,
    private pmService: PMService,
    private orgService: OrganizationService) {

  }

  ngOnInit(): void {

    this.serachForm = this.formBuilder.group({
      BudgetYear: ['', Validators.required],
      selectStructureId: ['', Validators.required],
      ReportBy: ['Quarter']
    })

    this.orgService.getOrgBranchSelectList().subscribe({
      next: (res) => {

        this.branchs = res
      }, error: (err) => {
        console.error(err)
      }
    })

    this.pmService.getProgramSelectList().subscribe({
      next: (res) => {
        this.programs = res
        console.log(res)
      }, error: (err) => {
        console.error(err)
      }
    })


  }


  exportTableToExcel(table: HTMLElement, fileName: string): void {
    const worksheet = XLSX.utils.table_to_sheet(table);
    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Sheet1');
    XLSX.writeFile(workbook, fileName + '.xlsx');
  }

  
  Search() {

    this.pmService.GetProgressReportByStructure(this.serachForm.value.BudgetYear, this.serachForm.value.ReportBy, this.serachForm.value.selectStructureId).subscribe({
      next: (res) => {

        console.log("progress st",res)
             this.progressReportByStructure = res 
            this.cnt = this.progressReportByStructure?.planDuration

      }, error: (err) => {
        console.error(err)
      }
    })

  }

  OnBranchChange(branchId: string) {

    this.orgService.getOrgStructureSelectList(branchId).subscribe({
      next: (res) => {
        this.structures = res

      }, error: (err) => {
        console.error(err)
      }
    })

  }

  range(length: number) {
    return Array.from({ length }, (_, i) => i);
  }



}