import { Component, OnInit } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { AddInsideCaseComponent } from './add-inside-case/add-inside-case.component';

@Component({
  selector: 'app-inside-case',
  templateUrl: './inside-case.component.html',
  styleUrls: ['./inside-case.component.css']
})
export class InsideCaseComponent implements OnInit {

  constructor (private modalService : NgbModal){

  }
  ngOnInit(): void {
    
  }

  getInsideCases(){

  }

  addCase() {
    let modalRef = this.modalService.open(AddInsideCaseComponent, { size: 'xl', backdrop: 'static' })
    modalRef.result.then(()=>{
      this.getInsideCases()
    })
  }
}
